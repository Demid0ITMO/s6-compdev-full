using Lab1;
using Lab2;

namespace Lab7
{
  public class Optimizer
  {
    public List<Statement> optimize(List<Statement> statements)
    {
      var optimized = new List<Statement>();
      foreach (var stmt in statements) {
        var opt = visitStatement(stmt);
        if (opt != null) optimized.Add(opt);
      }
      return optimized;
    }

    private Statement? visitStatement(Statement stmt)
    {
      switch (stmt) {
        case ExpressionStatement es:
          var expr = visitExpression(es.expression);
          return new ExpressionStatement(expr, es.row, es.column);

        case PrintStatement ps:
          var pExpr = visitExpression(ps.expression);
          return new PrintStatement(pExpr, ps.row, ps.column);

        case VarStatement vs:
          var init = vs.initializer != null ? visitExpression(vs.initializer) : null;
          return new VarStatement(vs.name, vs.postype, init, vs.row, vs.column);

        case BlockStatement bs:
          var newStmts = new List<Statement>();
          bool unreachable = false;

          foreach (var s in bs.statements) {
            if (unreachable) break;
            
            var optStmt = visitStatement(s);
            if (optStmt != null) {
              newStmts.Add(optStmt);
              if (optStmt is ReturnStatement) unreachable = true;
            }
          }
          return new BlockStatement(newStmts, bs.row, bs.column);

        case IfStatement ifs:
          var cond = visitExpression(ifs.condition);
          if (isConstant(cond)) {
            bool val = ((BooleanExpression)cond).value;
            
            if (val) {
              var thenOpt = visitStatement(ifs.thenBranch);
              return thenOpt;
            } else {
              if (ifs.elseBranch != null) return visitStatement(ifs.elseBranch);
              return null;
            }

          } else {
            var thenOpt = visitStatement(ifs.thenBranch);
            var elseOpt = ifs.elseBranch != null ? visitStatement(ifs.elseBranch) : null;
            return new IfStatement(cond, thenOpt ?? emptyBlock(ifs.row, ifs.column), elseOpt, ifs.row, ifs.column);
          }

        case WhileStatement ws:
          cond = visitExpression(ws.condition);
          if (isConstant(cond) && ((BooleanExpression)cond).value == false) return null;
          
          var bodyOpt = visitStatement(ws.body);
          return new WhileStatement(cond, bodyOpt ?? emptyBlock(ws.row, ws.column), ws.row, ws.column);

        case FuncDeclarationStatement fds:
          var fdBody = visitStatement(fds.body) as BlockStatement;
          return new FuncDeclarationStatement(fds.name, fds.args, fdBody ?? emptyBlock(fds.row, fds.column), fds.row, fds.column);

        case ReturnStatement rs:
          var retExpr = rs.expr != null ? visitExpression(rs.expr) : null;
          return new ReturnStatement(retExpr, rs.row, rs.column);

        default:
          return stmt;
      }
    }

    private Expression visitExpression(Expression expr)
    {
      switch (expr) {
        case BinaryExpression bin:
          var left = visitExpression(bin.first);
          var right = visitExpression(bin.second);
          
          if (isConstant(left) && isConstant(right)) return foldBinaryConst(left, right, bin.oper);
          
          var simplified = simplifyBinary(left, right, bin.oper);
          if (simplified != null) return simplified;
          
          return new BinaryExpression(left, right, bin.oper);

        case UnaryExpression un:
          var val = visitExpression(un.value);
          
          if (isConstant(val)) return foldUnaryConst(val, un.oper);
          
          if (un.oper == TokenType.NON && val is UnaryExpression inner && inner.oper == TokenType.NON) return inner.value;
          if (un.oper == TokenType.MINUS && val is UnaryExpression innerM && innerM.oper == TokenType.MINUS) return innerM.value;
          
          return new UnaryExpression(val, un.oper);

        case AssignExpression assign:
          val = visitExpression(assign.value);
          return new AssignExpression(assign.name, val);

        case FuncCallExpression fc:
          var newArgs = fc.args.Select(visitExpression).ToList();
          return new FuncCallExpression(fc.name, newArgs);

        default:
          return expr;
      }
    }

    private bool isConstant(Expression expr) => expr is NumberExpression || expr is StringExpression || expr is BooleanExpression;

    private object getConstValue(Expression expr)
    {
      if (expr is NumberExpression n) return n.value;
      if (expr is StringExpression s) return s.value;
      if (expr is BooleanExpression b) return b.value;
      return new object();
    }

    private Expression foldBinaryConst(Expression left, Expression right, TokenType op)
    {
      object l = getConstValue(left);
      object r = getConstValue(right);
      
      if (l is double ld && r is double rd) {
        switch (op) {
          case TokenType.PLUS: return new NumberExpression(ld + rd);
          case TokenType.MINUS: return new NumberExpression(ld - rd);
          case TokenType.MUL: return new NumberExpression(ld * rd);
          case TokenType.EQEQ: return new BooleanExpression(ld == rd);
          case TokenType.NONEQ: return new BooleanExpression(ld != rd);
          case TokenType.LT: return new BooleanExpression(ld < rd);
          case TokenType.RT: return new BooleanExpression(ld > rd);
          case TokenType.LTEQ: return new BooleanExpression(ld <= rd);
          case TokenType.RTEQ: return new BooleanExpression(ld >= rd);
        }
      } else if (l is string ls && r is string rs) {
        switch (op) {
          case TokenType.PLUS: return new StringExpression(ls + rs);
          case TokenType.EQEQ: return new BooleanExpression(ls == rs);
          case TokenType.NONEQ: return new BooleanExpression(ls != rs);
        }
      } else if (l is bool lb && r is bool rb) {
        switch (op) {
          case TokenType.EQEQ: return new BooleanExpression(lb == rb);
          case TokenType.NONEQ: return new BooleanExpression(lb != rb);
          case TokenType.AND: return new BooleanExpression(lb && rb);
          case TokenType.OR: return new BooleanExpression(lb || rb);
        }
      }
      
      return new BinaryExpression(left, right, op);
    }

    private Expression foldUnaryConst(Expression operand, TokenType op)
    {
      object val = getConstValue(operand);
      
      if (val is double d) {
        if (op == TokenType.MINUS) return new NumberExpression(-d);
        if (op == TokenType.NON) return new BooleanExpression(d == 0);
      } 
      else if (val is bool b) {
        if (op == TokenType.NON) return new BooleanExpression(!b);
      }
      else if (val is string s) {
        if (op == TokenType.NON) return new BooleanExpression(string.IsNullOrEmpty(s));
      }
      
      return new UnaryExpression(operand, op);
    }

    private Expression? simplifyBinary(Expression left, Expression right, TokenType op)
    {
      if (left is NumberExpression && right is NumberExpression) {
        var l = (NumberExpression)left;
        var r = (NumberExpression)right;
        
        if (op == TokenType.PLUS) { // a + 0 = a
          if (l.value == 0) return r;
          if (r.value == 0) return l;
        }
        if (op == TokenType.MINUS) { // a - 0 = a
          if (r.value == 0) return l;
        }
        if (op == TokenType.MUL) { // a * 1 = a , a * 0 = 0
          if (l.value == 0 || r.value == 0) return new NumberExpression(0);
          if (l.value == 1) return r;
          if (r.value == 1) return l;
        }
        if (op == TokenType.DIV) { // a / 1 = a
          if (r.value == 1) return l;
        }
      }

      if (left is BooleanExpression && right is BooleanExpression) {
        var l = (BooleanExpression)left;
        var r = (BooleanExpression)right;

        if (op == TokenType.AND) { // a && false = false, a && true = a
          if (l.value == false || r.value == false) return new BooleanExpression(false);
          if (l.value == true) return r;
          if (r.value == true) return l;
        }
        if (op == TokenType.OR) { // a || true = true, a || false = a
          if (l.value == true || r.value == true) return new BooleanExpression(true);
          if (l.value == false) return r;
          if (r.value == false) return l;
        } 
      }
      
      return null;
    }

    private BlockStatement emptyBlock(int row, int col) => new BlockStatement(new List<Statement>(), row, col);
  }
}
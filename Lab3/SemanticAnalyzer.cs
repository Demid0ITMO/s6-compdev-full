using Lab1;
using Lab2;

namespace Lab3
{
  public class SemanticAnalyzer
  {
    private class SemanticErr(string message, int row, int column)
    {
        public string msg { get; } = message;
        public int r { get; } = row;
        public int c { get; } = column;
    };
    private SemanticEnv env = new();
    private readonly List<SemanticErr> errs = [];
    private readonly List<SemanticErr> warns = [];
    public IEnumerable<string> errors => errs
        .OrderBy(err => err.r)
        .ThenBy(err => err.c)
        .Select(err => $"Semantic Error [{err.r}:{err.c}]: {err.msg}");
    public IEnumerable<string> warnings => warns
        .OrderBy(err => err.r)
        .ThenBy(err => err.c)
        .Select(err => $"Semantic Warning [{err.r}:{err.c}]: {err.msg}");

    public IEnumerable<string> errnwarn => errors
    .Zip(errs, (a, b) => new SemanticErr(a, b.r, b.c))
    .Concat(warnings
        .Zip(warns, (a, b) => new SemanticErr(a, b.r, b.c))
    )
    .DistinctBy(err => $"{err.r}:{err.c}")
    .OrderBy(e => e.r)
    .ThenBy(e => e.c)
    .Select(e => e.msg);

    public void analyze(IEnumerable<Statement> statements)
    {
      foreach (var statement in statements) {
        var t = visitStmt(statement);
        if (t == DataType.UNKNOWN) errs.Add(new SemanticErr("Unknown type", statement.row, statement.column));
        else if (t != DataType.VOID) warns.Add(new SemanticErr($"Ignoring statement ouput {t}", statement.row, statement.column));
      }
      checkUnusedVariables(1, 1);
    }

    public DataType visitStmt(Statement statement)
    {
        switch (statement)
        {
            case VarStatement v: analyzeVarStatement(v); return DataType.VOID;
            case PrintStatement p: analyzePrintStatement(p); return DataType.VOID;
            case ExpressionStatement e: return analyzeExpressionStatement(e);
            case BlockStatement b: analyzeBlockStatement(b); return DataType.VOID;
            case IfStatement i: analyzeIfStatement(i); return DataType.VOID;
            case WhileStatement w: analyzeWhileStatement(w); return DataType.VOID;
            default:
                errs.Add(new SemanticErr($"Unsupported statement {statement.GetType().Name}", statement.row, statement.column));
                return DataType.VOID;
        }
    }

    public DataType visitExpr(Expression expression, int r, int c)
    {
        switch (expression)
        {
            case NumberExpression n: return DataType.NUM;
            case StringExpression s: return DataType.STR;
            case BooleanExpression b: return DataType.BOOL;
            case VariableExpression v: return analyzeVariableExpression(v, r, c);
            case AssignExpression a: return analyzeAssignExpression(a, r, c);
            case BinaryExpression b: return analyzeBinaryExpression(b, r, c);
            case UnaryExpression u: return analyzeUnaryExpression(u, r, c);
            default:
                errs.Add(new SemanticErr($"Unsupported expression {expression.GetType().Name}", r, c));
                return DataType.VOID;
        }
    }

    private void analyzeVarStatement(VarStatement stmt)
    {
        if (!env.defineVar(stmt.name, false, null))
        {
            if (env.getVar(stmt.name)?.isInited ?? false) errs.Add(new SemanticErr($"Var '{stmt.name}' already defined", stmt.row, stmt.column));
            else errs.Add(new SemanticErr($"Var '{stmt.name}' already declared", stmt.row, stmt.column));
        }

        else if (stmt.initializer != null)
        {
            var type = visitExpr(stmt.initializer, stmt.row, stmt.column);
            env.setInited(stmt.name, type);
        }
    }

    private void analyzePrintStatement(PrintStatement stmt)
    {
        visitExpr(stmt.expression, stmt.row, stmt.column);
    }

    private DataType analyzeExpressionStatement(ExpressionStatement stmt)
    {
        return visitExpr(stmt.expression, stmt.row, stmt.column);
    }

    private void analyzeBlockStatement(BlockStatement stmt)
    {
        var prevEnv = env;
        env = new SemanticEnv(prevEnv);

        foreach (var innerStatement in stmt.statements)
        {
            var t = visitStmt(innerStatement);
            if (t == DataType.UNKNOWN) errs.Add(new SemanticErr("Unknown type", innerStatement.row, innerStatement.column));
            else if (t != DataType.VOID) warns.Add(new SemanticErr("Ignoring function ouput", innerStatement.row, innerStatement.column));
        }

        checkUnusedVariables(stmt.row, stmt.column);
        env = prevEnv;
    }

    private void analyzeIfStatement(IfStatement stmt)
    {
        var t = visitExpr(stmt.condition, stmt.row, stmt.column);
        var t1 = visitStmt(stmt.thenBranch);
        DataType t2 = DataType.VOID;

        if (stmt.elseBranch != null)
        {
            t2 = visitStmt(stmt.elseBranch);
        }

        if (t != DataType.BOOL) errs.Add(new SemanticErr("Condition must be boolean", stmt.row, stmt.column));
        if (t1 != DataType.VOID) warns.Add(new SemanticErr("Ignoring function ouput", stmt.thenBranch.row, stmt.thenBranch.column));
        if (t2 != DataType.VOID) warns.Add(new SemanticErr("Ignoring function ouput", stmt.elseBranch?.row ?? stmt.row, stmt.elseBranch?.column ?? stmt.row));
    }

    private void analyzeWhileStatement(WhileStatement stmt)
    {
        var t = visitExpr(stmt.condition, stmt.row, stmt.column);
        var t1 = visitStmt(stmt.body);

        if (t != DataType.BOOL) errs.Add(new SemanticErr("Condition must be boolean", stmt.row, stmt.column));
        if (t1 != DataType.VOID) warns.Add(new SemanticErr("Ignoring function ouput", stmt.body.row, stmt.body.column));
    }

    private void checkUnusedVariables(int r, int c)
    {
        foreach (var symbol in env.GetLocalVariables())
        {
            if (!symbol.isUsed)
            {
                if (symbol.isInited) warns.Add(new SemanticErr($"Var '{symbol.name}' is defined, but not used", r, c));
                else warns.Add(new SemanticErr($"Var '{symbol.name}' is declared, but not used", r, c));
            }
        }
    }

    private DataType analyzeVariableExpression(VariableExpression expr, int r, int c)
    {
        var symbol = env.getVar(expr.name);
        if (symbol == null)
        {
            errs.Add(new SemanticErr($"Var '{expr.name}' is undeclared", r, c));
        }
        else
        {
            symbol.isUsed = true;

            if (!symbol.isInited)
            {
                errs.Add(new SemanticErr($"Var '{expr.name}' is undefined", r, c));
            }
        }
        return symbol?.type ?? DataType.UNKNOWN;
    }

    private DataType analyzeAssignExpression(AssignExpression expr, int r, int c)
    {
        DataType type = visitExpr(expr.value, r, c);

        if (!env.isVarDefined(expr.name))
        {
            errs.Add(new SemanticErr($"Var '{expr.name}' is undeclared", r, c));
        }
        else
        {
            var v = env.getVar(expr.name);
            if ((v?.isInited ?? false) && (type != v.type)) errs.Add(new SemanticErr($"Var '{expr.name}' has type '{v.type}', but assigned expression has type '{type}'", r, c));
            else if (type != DataType.UNKNOWN) env.setInited(expr.name, type);
        }

        return DataType.VOID;
    }

    private DataType analyzeBinaryExpression(BinaryExpression expr, int r, int c)
    {
        var t1 = visitExpr(expr.first, r, c);
        var t2 = visitExpr(expr.second, r, c);
        
        if (t1 == DataType.UNKNOWN || t2 == DataType.UNKNOWN) {
            errs.Add(new SemanticErr("Operand has unknown type", r, c));
            return DataType.UNKNOWN;
        }
        
        if (t1 != t2) {
            errs.Add(new SemanticErr($"Unsupported operation between '{t1}' and '{t2}'", r, c));
            return DataType.UNKNOWN;
        }

        List<TokenType> nums_to_num_opers = [
            TokenType.PLUS, 
            TokenType.MINUS, 
            TokenType.MUL, 
            TokenType.DIV,
        ];
        if (t1 == DataType.NUM && nums_to_num_opers.Contains(expr.oper)) return DataType.NUM;

        List<TokenType> nums_to_bool_opers = [
            TokenType.EQEQ,
            TokenType.NONEQ,
            TokenType.LT,
            TokenType.RT,
            TokenType.LTEQ,
            TokenType.RTEQ
        ];
        if (t1 == DataType.NUM && nums_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;

        List<TokenType> strs_to_str_opers = [
            TokenType.PLUS,
        ];
        if (t1 == DataType.STR && strs_to_str_opers.Contains(expr.oper)) return DataType.STR;

        List<TokenType> strs_to_bool_opers = [
            TokenType.EQEQ,
            TokenType.NONEQ,
        ];
        if (t1 == DataType.STR && strs_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;

        List<TokenType> bools_to_bool_opers = [
            TokenType.AND,
            TokenType.OR,
            TokenType.PLUS,
            TokenType.MUL,
            TokenType.EQEQ,
            TokenType.NONEQ
        ];     
        if (t1 == DataType.BOOL && bools_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;

        errs.Add(new SemanticErr($"Unsupported operation between '{t1}' and '{t2}'", r, c));
        return DataType.UNKNOWN;
    }

    private DataType analyzeUnaryExpression(UnaryExpression expr, int r, int c)
    {
        var t = visitExpr(expr.value, r, c);
        if (expr.oper == TokenType.MINUS && t == DataType.NUM) return DataType.NUM;
        if (expr.oper == TokenType.NON && t == DataType.BOOL) return DataType.BOOL;
        return DataType.UNKNOWN;
    }
  }
}
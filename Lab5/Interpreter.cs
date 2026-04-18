using System.Runtime.InteropServices.Marshalling;
using Lab1;
using Lab2;

/**
runtime env
{
dict
get
set
}

tree interpret
{
eval(expr);
exec(stmt);
}
*/
namespace Lab5
{

  public class RuntimeEnv
  {
    private Dictionary<String, Object?> variables = [];

    public Object? getVar(String name)
    {
      return variables[name];
    }

    public void setVar(String name, Object? value)
    {
      variables[name] = value;
    }

    public Dictionary<String, Object?> getDict()
    {
      return variables;
    }
  }

  public class Interpreter
  {
    private RuntimeEnv runtimeEnv = new RuntimeEnv();
    
    public Object evaluateExpression(Expression expr)
    {
      switch(expr)
      {
        case NumberExpression n: return n.value;
        case StringExpression s: return s.value;
        case BooleanExpression b: return b.value;
        case VariableExpression v: return runtimeEnv.getVar(v.name) ?? new object();
        case AssignExpression a: 
          runtimeEnv.setVar(a.name, evaluateExpression(a.value));
          return new object();
        case BinaryExpression b: 
          var first = evaluateExpression(b.first);
          var second = evaluateExpression(b.second);
          if (first is Double)
          {
            switch(b.oper)
            {
              case TokenType.PLUS: return (Double)first + (Double)second;
              case TokenType.MINUS: return (Double)first - (Double)second;
              case TokenType.MUL: return (Double)first * (Double)second;
              case TokenType.DIV: 
                if ((Double)second == 0) throw new Exception("Interpreter Exception: Divided by zero");
                return (Double)first / (Double)second;
              case TokenType.EQEQ: return (Double)first == (Double)second;
              case TokenType.NONEQ: return (Double)first != (Double)second;
              case TokenType.LT: return (Double)first < (Double)second;
              case TokenType.RT: return (Double)first > (Double)second;
              case TokenType.LTEQ: return (Double)first <= (Double)second;
              case TokenType.RTEQ: return (Double)first >= (Double)second;
            }
          }
          else if (first is String)
          {
            switch(b.oper)
            {
              case TokenType.PLUS: return (String)first + (String)second;
              case TokenType.EQEQ: return (String)first == (String)second;
              case TokenType.NONEQ: return (String)first != (String)second;
            }
          }
          else if (first is Boolean)
          {
            switch(b.oper)
            {
              case TokenType.PLUS: return (Boolean)first || (Boolean)second;
              case TokenType.MUL: return (Boolean)first && (Boolean)second;
              case TokenType.EQEQ: return (Boolean)first == (Boolean)second;
              case TokenType.NONEQ: return (Boolean)first != (Boolean)second;
              case TokenType.AND: return (Boolean)first && (Boolean)second;
              case TokenType.OR: return (Boolean)first || (Boolean)second;
            }
          }
          return new object();
        case UnaryExpression u:
          var value = evaluateExpression(u.value);
          if (u.oper == TokenType.NON) return !(Boolean)value;
          if (u.oper == TokenType.MINUS) return -(Double)value;
          return new object();
        default: return new object();
      }
    }

    public void executeStatement(Statement stmt)
    {
      switch(stmt)
      {
        case ExpressionStatement e: 
          evaluateExpression(e.expression);
          return;
        case PrintStatement p:
          var o = evaluateExpression(p.expression);
          Console.WriteLine(o);
          return;
        case VarStatement v:
          if (v.initializer == null) runtimeEnv.setVar(v.name, null);
          else runtimeEnv.setVar(v.name, evaluateExpression(v.initializer));
          return;
        case BlockStatement b:
          foreach (Statement st in b.statements) executeStatement(st);
          return;
        case IfStatement i:
          var cond = evaluateExpression(i.condition);
          if ((Boolean)cond) executeStatement(i.thenBranch);
          else if (i.elseBranch != null) executeStatement(i.elseBranch);
          return;
        case WhileStatement w:
          while ((Boolean)evaluateExpression(w.condition)) executeStatement(w.body);
          return;
      }
    }

    public String runtimeEnvDump()
    {
      var ans = "Variables {\n";
      foreach (var v in runtimeEnv.getDict())
      {
        if (v.Value is Double) ans += $"- Double {v.Key} : {v.Value}\n";
        else if (v.Value is String) ans += $"- String {v.Key} : \"{v.Value}\"\n";
        else if (v.Value is Boolean) ans += $"- Boolean {v.Key} : {v.Value}\n";
        else ans += $"- Unknown {v.Key} : {v.Value ?? "NULL"}\n";
      }
      ans += "}";
      return ans;
    }
  }
}
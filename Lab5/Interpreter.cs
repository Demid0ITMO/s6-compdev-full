using Lab1;
using Lab2;
using Lab3;

namespace Lab5
{

  public class FuncEnv(List<NameTypePair> ar, BlockStatement b)
  {
    public List<NameTypePair> args { get; set; } = ar;
    public BlockStatement body { get; set; } = b;
  }

  public class RuntimeEnv
  {
    private readonly RuntimeEnv? parent;
    
    private readonly Dictionary<String, Object?> variables = [];

    private readonly Dictionary<String, FuncEnv> functions = [];

    private readonly Dictionary<String, bool> isArrayVar = [];

    public RuntimeEnv(RuntimeEnv? par = null)
    {
      parent = par;
    }

    public Object? getVar(String name)
    {
      variables.TryGetValue(name, out var variable);
      if (variable == null)
      {
        if (parent == null) return null;
        else return parent.getVar(name);
      }
      else return variable;
    }

    public void setVar(String name, Object? value, bool isArray = false)
    {
      variables[name] = value;
      if (isArray) isArrayVar[name] = true;
    }

    public bool isArray(String name)
    {
      if (isArrayVar.ContainsKey(name)) return true;
      if (parent != null) return parent.isArray(name);
      return false;
    }

    public FuncEnv? getFunc(String name)
    {
      functions.TryGetValue(name, out var func);
      if (func == null)
      {
        if (parent == null) return null;
        else return parent.getFunc(name);
      }
      else return func;
    }

    public void setFunc(String name, List<NameTypePair> args, BlockStatement body)
    {
      functions[name] = new FuncEnv(args, body);
    }

    public Dictionary<String, Object?> getDict()
    {
      return variables;
    }

    public Dictionary<String, FuncEnv> getFuncDict()
    {
      return functions;
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
        case VariableExpression v: return runtimeEnv.getVar(v.name) ?? throw new Exception($"Unable to find variable '{v.name}'");
        case AssignExpression assign:
          var val = evaluateExpression(assign.value);
          if (assign.target is VariableExpression varTarget) {
            runtimeEnv.setVar(varTarget.name, val);
          } else if (assign.target is ArrayIndexExpression arrTarget) {
            var arrObj2 = runtimeEnv.getVar(arrTarget.arrayName) ?? throw new Exception($"Array '{arrTarget.arrayName}' is not defined");
            
            if (!runtimeEnv.isArray(arrTarget.arrayName)) throw new Exception($"'{arrTarget.arrayName}' is not an array");

            var list2 = arrObj2 as List<object> ?? throw new Exception($"'{arrTarget.arrayName}' is not a list");
            
            var idxObj2 = evaluateExpression(arrTarget.index);
            if (idxObj2 is not double idxVal2) throw new Exception("Array index must be a number");
            
            int index2 = (int)idxVal2;
            if (index2 < 0) throw new Exception($"Index {index2} out of bounds for array '{arrTarget.arrayName}'");
            
            while (list2.Count <= index2) list2.Add(null);
            list2[index2] = val;
          } else {
            throw new Exception($"Invalid assignment target");
          }
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
              case TokenType.EQEQ: return (Boolean)first == (Boolean)second;
              case TokenType.NONEQ: return (Boolean)first != (Boolean)second;
              case TokenType.AND: return (Boolean)first && (Boolean)second;
              case TokenType.OR: return (Boolean)first || (Boolean)second;
            }
          }
          throw new Exception($"Unknown binary operation");
        case UnaryExpression u:
          var value = evaluateExpression(u.value);
          if (u.oper == TokenType.NON) return !(Boolean)value;
          if (u.oper == TokenType.MINUS) return -(Double)value;
          throw new Exception($"Unknown unary operation");
        case FuncCallExpression fc:
          var func = runtimeEnv.getFunc(fc.name);
          if (func == null) throw new Exception($"Function '{fc.name}' is not defined");
          if (func.args.Count != fc.args.Count) throw new Exception($"Function '{fc.name}' called with {fc.args.Count} args, but expected {func.args.Count}");

          var prevEnv = runtimeEnv;
          var newEnv = new RuntimeEnv(prevEnv);
          for(int i = 0; i < func.args.Count; i++) {
            var arg = func.args[i];
            val = evaluateExpression(fc.args[i]);
            if (arg.type == TokenType.BOOLTYPE) val = (bool)val;
            if (arg.type == TokenType.STRTYPE) val = (string)val;
            if (arg.type == TokenType.NUMTYPE) val = (double)val;
            newEnv.setVar(arg.name, val);
          }
          runtimeEnv = newEnv;
          object retVal = new object();
          try {
            executeStatement(func.body);
          } catch (ReturnException e)
          {
            if (e.value != null) retVal = e.value;
          } finally {
            runtimeEnv = prevEnv;
          }
          return retVal;

        case ArrayIndexExpression aIdx:
          var arrObj = runtimeEnv.getVar(aIdx.arrayName);
          if (arrObj is not List<object> list) throw new Exception($"'{aIdx.arrayName}' is not an array");
          
          var idxObj = evaluateExpression(aIdx.index);
          if (idxObj is not double idxVal) throw new Exception("Index must be a number");
          
          int index = (int)idxVal;
          if (index < 0 || index >= list.Count) throw new Exception($"Index {index} out of bounds for array '{aIdx.arrayName}'");
          
          return list[index];

        case ArrayLiteralExpression arrLit:
          list = new List<object>();
          foreach (var el in arrLit.elements) list.Add(evaluateExpression(el));
          return list;

        default: throw new Exception($"Unknown expression");
      }
    }

    public void executeStatement(Statement stmt)
    {
      try {
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
            if (v.initializer == null) {
              if (v.isArray) runtimeEnv.setVar(v.name, new List<object>(), true);
              else runtimeEnv.setVar(v.name, null);
            } else {
              var val = evaluateExpression(v.initializer);
              runtimeEnv.setVar(v.name, val, v.isArray);
            }
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
          case FuncDeclarationStatement fcs:
            runtimeEnv.setFunc(fcs.name, fcs.args, fcs.body);
            return;
          case ReturnStatement r:
            var re = new ReturnException();
            if (r.expr != null) re.value = evaluateExpression(r.expr);
            throw re;
        }
      } catch (Exception e)
      {
        if (e is ReturnException) throw e;
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"[RUNTIME ERROR]: {e.Message}");
          Console.ResetColor();
        }
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
      ans += "}\nFunctions {\n";
      foreach (var f in runtimeEnv.getFuncDict())
      {
        ans += $"{f.Key} - argc: {f.Value.args.Count}\n";
      }
      ans += "}";
      return ans;
    }
  }
}
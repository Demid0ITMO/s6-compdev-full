using Lab1;

namespace Lab2 
{
  public abstract class Expression {}

  public class TypedVariableExpression : VariableExpression
  {
    public TokenType type { get; }
    public TypedVariableExpression(string name, TokenType type) : base(name)
    {
      this.type = type;
    }
  }

  public class NumberExpression : Expression
  {
    public double value { get; }
    public NumberExpression(double val) => value = val;

    override public String ToString()
    {
      return $"{value}";
    }
  }

  public class StringExpression : Expression
  {
    public string value { get; }
    public StringExpression(string val) => value = val;

    override public String ToString()
    {
      return $"{value}";
    }
  }

  public class BooleanExpression : Expression
  {
    public bool value { get; }
    public BooleanExpression(bool val) => value = val;

    override public String ToString()
    {
      return $"{value}";
    }
  }

  public class VariableExpression : Expression
  {
    public string name { get; }
    public VariableExpression(string name) => this.name = name;

    override public String ToString()
    {
      return $"{name}";
    }
  }

  public class BinaryExpression : Expression
  {
    public Expression first { get; }
    public Expression second { get; }
    public TokenType oper { get; }

    public BinaryExpression(Expression f, Expression s, TokenType op)
    {
      first = f;
      second = s;
      oper = op;
    }

    override public String ToString()
    {
      return $"{first} {oper} {second}";
    }
  }

  public class UnaryExpression : Expression
  {
    public Expression value { get; }
    public TokenType oper { get; }

    public UnaryExpression(Expression val, TokenType op)
    {
      oper = op;
      value = val;
    }

    override public String ToString()
    {
      return $"{oper} {value}";
    }
  }

  public class AssignExpression : Expression
  {
    public string name { get; }
    public Expression value { get; }

    public AssignExpression(string name, Expression val)
    {
      this.name = name;
      value = val;
    }

    override public String ToString()
    {
      return $"{name} = {value}";
    }
  }

  public class FuncCallExpression: Expression
  {
    public string name { get; }
    public List<Expression> args { get; }
    public FuncCallExpression(string name, List<Expression> args)
    {
      this.name = name;
      this.args = args;
    }

    override public String ToString()
    {
      var argsLine = "";
      for (int i = 0; i < args.Count; i++)
      {
        argsLine += args[i].ToString();
        if (args.Count != i + 1) argsLine += ", ";
      }
      return $"{name} ({argsLine})";
    }
  }
}
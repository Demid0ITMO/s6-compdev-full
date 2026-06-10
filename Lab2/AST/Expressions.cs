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
    public Expression target { get; }
    public Expression value { get; }


    public AssignExpression(Expression tar, Expression val)
    {
      target = tar;
      value = val;
    }

    public string name => (target as VariableExpression)?.name ?? "";

    override public string ToString()
    {
      return $"{target} = {value}";
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

  public class ArrayIndexExpression : Expression
  {
    public string arrayName { get; }
    public Expression index { get; }
    public ArrayIndexExpression(string name, Expression idx)
    {
      arrayName = name;
      index = idx;
    }
    override public string ToString() => $"{arrayName}[{index}]";
  }

  public class ArrayLiteralExpression : Expression
  {
    public List<Expression> elements { get; }
    public ArrayLiteralExpression(List<Expression> elems)
    {
      elements = elems;
    }
    override public string ToString()
    {
      return "[" + string.Join(", ", elements) + "]";
    }
  }
}
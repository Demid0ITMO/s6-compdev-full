using Lab1;

namespace Lab2 
{
  public abstract class Expression {}

  public class NumberExpression : Expression
  {
    public double value { get; }
    public NumberExpression(double val) => value = val;
  }

  public class StringExpression : Expression
  {
    public string value { get; }
    public StringExpression(string val) => value = val;
  }

  public class BooleanExpression : Expression
  {
    public bool value { get; }
    public BooleanExpression(bool val) => value = val;
  }

  public class VariableExpression : Expression
  {
    public string name { get; }
    public VariableExpression(string name) => this.name = name;
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
  }
}
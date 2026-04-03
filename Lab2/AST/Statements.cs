namespace Lab2
{
  public abstract class Statement {}
  
  public class ExpressionStatement : Statement
  {
    public Expression expression { get; }
    public ExpressionStatement(Expression expr) => expression = expr;
  }

  public class PrintStatement : Statement
  {
    public Expression expression { get; }
    public PrintStatement(Expression expr) => expression = expr;
  }


  public class VarStatement : Statement
  {
    public string name { get; }
    public Expression? initializer { get; }

    public VarStatement(string name, Expression? init)
    {
      this.name = name;
      initializer = init;
    }
  }

  public class BlockStatement : Statement
  {
    public List<Statement> statements { get; }
    public BlockStatement(List<Statement> states) => statements = states;
  }

  public class IfStatement : Statement
  {
    public Expression condition { get; }
    public Statement thenBranch { get; }
    public Statement? elseBranch { get; }
    public IfStatement(Expression cond, Statement thenB, Statement? elseB)
    {
      condition = cond;
      thenBranch = thenB;
      elseBranch = elseB;
    }
  }

  public class WhileStatement : Statement
  {
    public Expression condition { get; }
    public Statement body { get; }

    public WhileStatement(Expression cond, Statement block)
    {
      condition = cond;
      body = block;
    }
  }
}
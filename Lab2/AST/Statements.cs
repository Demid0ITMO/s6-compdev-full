using Lab1;

namespace Lab2
{
  public abstract class Statement(int r, int c)
  {
    public int row { get; } = r;
    public int column { get; } = c;
  }
  
  public class ExpressionStatement : Statement
  {
    public Expression expression { get; }
    public ExpressionStatement(Expression expr, int r, int c) : base(r, c) => expression = expr;
  }

  public class PrintStatement : Statement
  {
    public Expression expression { get; }
    public PrintStatement(Expression expr, int r, int c) : base(r, c) => expression = expr;
  }


  public class VarStatement : Statement
  {
    public string name { get; }
    public TokenType postype { get; }
    public bool isArray { get; }
    public Expression? initializer { get; }

    public VarStatement(string name, TokenType postype, bool isArray, Expression? init, int r, int c) : base(r, c)
    {
      this.name = name;
      this.postype = postype;
      this.isArray = isArray;
      initializer = init;
    }
  }

  public class BlockStatement : Statement
  {
    public List<Statement> statements { get; }
    public BlockStatement(List<Statement> states, int r, int c) : base(r, c) => statements = states;
  }

  public class IfStatement : Statement
  {
    public Expression condition { get; }
    public Statement thenBranch { get; }
    public Statement? elseBranch { get; }
    public IfStatement(Expression cond, Statement thenB, Statement? elseB, int r, int c) : base(r, c)
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

    public WhileStatement(Expression cond, Statement block, int r, int c) : base(r, c)
    {
      condition = cond;
      body = block;
    }
  }

  public class FuncDeclarationStatement: Statement
  {
    public string name { get; }
    public List<NameTypePair> args { get; }
    public BlockStatement body { get; }
    
    public FuncDeclarationStatement(string name, List<NameTypePair> args, BlockStatement body, int r, int c) : base(r, c)
    {
      this.name = name;
      this.args = args;
      this.body = body;
    }
  }

  public class ReturnStatement: Statement
  {
    public Expression? expr { get; }
    public ReturnStatement(Expression? expr, int r, int c) : base(r, c)
    {
      this.expr = expr;
    }
  }
}
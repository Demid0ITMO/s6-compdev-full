using Lab2;

namespace Lab3
{
  public class SemanticAnalyzer
  {
    private SemanticEnv env = new();
    private readonly List<string> errs = [];
    public IEnumerable<string> errors => errs;

    public void analyze(IEnumerable<Statement> statements)
    {
      foreach (var statement in statements) visitStmt(statement);
    }

    public void visitStmt(Statement statement)
    {
        switch (statement)
        {
            case VarStatement v: analyzeVarStatement(v); break;
            case PrintStatement p: analyzePrintStatement(p); break;
            case ExpressionStatement e: analyzeExpressionStatement(e); break;
            case BlockStatement b: analyzeBlockStatement(b); break;
            case IfStatement i: analyzeIfStatement(i); break;
            case WhileStatement w: analyzeWhileStatement(w); break;
            default:
                errs.Add($"Semantic error [{statement.row}:{statement.column}]: Unsupported statement {statement.GetType().Name}");
                break;
        }
    }

    public void visitExpr(Expression expression)
    {
        switch (expression)
        {
            case NumberExpression n: break;
            case StringExpression s: break;
            case VariableExpression v: analyzeVariableExpression(v); break;
            case AssignExpression a: analyzeAssignExpression(a); break;
            case BinaryExpression b: analyzeBinaryExpression(b); break;
            case UnaryExpression u: analyzeUnaryExpression(u); break;
            default:
                errs.Add($"Semantic error: Unsupported expression {expression.GetType().Name}");
                break;
        }
    }

    private void analyzeVarStatement(VarStatement stmt)
    {
        if (!env.defineVar(stmt.name, false))
        {
            errs.Add($"Semantic error [{stmt.row}:{stmt.column}]: Var '{stmt.name}' already defined");
        }

        if (stmt.initializer != null)
        {
            visitExpr(stmt.initializer);
            env.setInited(stmt.name);
        }
    }

    private void analyzePrintStatement(PrintStatement stmt)
    {
        visitExpr(stmt.expression);
        checkUnusedVariables();
    }

    private void analyzeExpressionStatement(ExpressionStatement stmt)
    {
        visitExpr(stmt.expression);
    }

    private void analyzeBlockStatement(BlockStatement stmt)
    {
        var prevEnv = env;
        env = new SemanticEnv(prevEnv);

        foreach (var innerStatement in stmt.statements)
        {
            visitStmt(innerStatement);
        }

        checkUnusedVariables();
        env = prevEnv;
    }

    private void analyzeIfStatement(IfStatement stmt)
    {
        visitExpr(stmt.condition);
        visitStmt(stmt.thenBranch);

        if (stmt.elseBranch != null)
        {
            visitStmt(stmt.elseBranch);
        }
    }

    private void analyzeWhileStatement(WhileStatement stmt)
    {
        visitExpr(stmt.condition);
        visitStmt(stmt.body);
    }

    private void checkUnusedVariables()
    {
        foreach (var symbol in env.GetLocalVariables())
        {
            if (!symbol.isUsed)
            {
                errs.Add($"Semantic Warning: Var '{symbol.name}' is defined, but not used");
            }
        }
    }

    private void analyzeVariableExpression(VariableExpression expr)
    {
        var symbol = env.getVar(expr.name);
        if (symbol == null)
        {
            errs.Add($"Semantic Error: Var '{expr.name}' is undeclared");
        }
        else
        {
            symbol.isUsed = true;

            if (!symbol.isInited)
            {
                errs.Add($"Semantic Error: Var '{expr.name}' is undefined");
            }
        }
    }

    private void analyzeAssignExpression(AssignExpression expr)
    {
        visitExpr(expr.value);

        if (!env.isVarDefined(expr.name))
        {
            errs.Add($"Semantic Error: Var '{expr.name}' is undeclared");
        }
        else
        {
            env.setInited(expr.name);
        }
    }

    private void analyzeBinaryExpression(BinaryExpression expr)
    {
        visitExpr(expr.first);
        visitExpr(expr.second);
    }

    private void analyzeUnaryExpression(UnaryExpression expr)
    {
        visitExpr(expr.value);
    }
  }
}
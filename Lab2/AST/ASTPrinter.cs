namespace Lab2
{
  public class AstPrinter
  {
    public void Print(List<Statement> statements)
    {
      Console.WriteLine("Root (Program)");
      for (int i = 0; i < statements.Count; i++)
      {
        PrintNode(statements[i], "", i == statements.Count - 1);
      }
    }

    private void PrintNode(object node, string indent, bool isLast)
    {
      if (node == null) return;

      string marker = isLast ? "└── " : "├── ";
      Console.Write(indent + marker);

      string childIndent = indent + (isLast ? "    " : "│   ");

      switch (node)
      {
        case VarStatement v:
          Console.WriteLine($"VarStatement: {v.name}");
          if (v.initializer != null) PrintNode(v.initializer, childIndent, true);
          break;

        case PrintStatement p:
          Console.WriteLine("PrintStatement");
          PrintNode(p.expression, childIndent, true);
          break;

        case IfStatement i:
          Console.WriteLine("IfStatement");
          PrintNode(i.condition, childIndent, false);
          PrintNode(i.thenBranch, childIndent, i.elseBranch == null);

          if (i.elseBranch != null) PrintNode(i.elseBranch, childIndent, true);
          break;

        case WhileStatement w:
          Console.WriteLine("WhileStatement");
          PrintNode(w.condition, childIndent, false);
          PrintNode(w.body, childIndent, true);
          break;

        case BlockStatement b:
          Console.WriteLine("BlockStatement");
          for (int j = 0; j < b.statements.Count; j++) PrintNode(b.statements[j], childIndent, j == b.statements.Count - 1);
          break;

        case ExpressionStatement e:
          Console.WriteLine("ExpressionStatement");
          PrintNode(e.expression, childIndent, true);
          break;

        case BinaryExpression bin:
          Console.WriteLine($"BinaryExpression: {bin.oper}");
          PrintNode(bin.first, childIndent, false);
          PrintNode(bin.second, childIndent, true);
          break;

        case UnaryExpression un:
          Console.WriteLine($"UnaryExpression: {un.oper}");
          PrintNode(un.value, childIndent, true);
          break;

        case AssignExpression assign:
          Console.WriteLine($"AssignExpression: {assign.name} =");
          PrintNode(assign.value, childIndent, true);
          break;

        case NumberExpression num:
          Console.WriteLine($"Number: {num.value}");
          break;

        case VariableExpression varExpr:
          Console.WriteLine($"Variable: {varExpr.name}");
          break;

        default:
          Console.WriteLine($"Unknown Node: {node.GetType().Name}");
          break;
      }
    }
  }
}
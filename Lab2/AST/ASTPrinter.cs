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
          Console.WriteLine($"VarStatement [{v.row}:{v.column}]: {(v.isArray ? "array " : "")}{v.name}");
          if (v.initializer != null) PrintNode(v.initializer, childIndent, true);
          break;

        case FuncDeclarationStatement fds:
          string argLine = "";
          for (int i = 0; i < fds.args.Count; i++)
          {
            argLine += fds.args[i];
            if (i + 1 != fds.args.Count) argLine += ", ";
          }

          Console.WriteLine($"FuncDeclarationStatement [{fds.row}:{fds.column}]: {fds.name}({argLine})");
          PrintNode(fds.body, childIndent, true);
          break;

        case ReturnStatement r:
          Console.WriteLine($"ReturnStatement [{r.row}:{r.column}]");
          if (r.expr != null) PrintNode(r.expr, childIndent, true);
          break;

        case PrintStatement p:
          Console.WriteLine($"PrintStatement [{p.row}:{p.column}]");
          PrintNode(p.expression, childIndent, true);
          break;

        case IfStatement i:
          Console.WriteLine($"IfStatement [{i.row}:{i.column}]");
          PrintNode(i.condition, childIndent, false);
          PrintNode(i.thenBranch, childIndent, i.elseBranch == null);

          if (i.elseBranch != null) PrintNode(i.elseBranch, childIndent, true);
          break;

        case WhileStatement w:
          Console.WriteLine($"WhileStatement [{w.row}:{w.column}]");
          PrintNode(w.condition, childIndent, false);
          PrintNode(w.body, childIndent, true);
          break;

        case BlockStatement b:
          Console.WriteLine($"BlockStatement [{b.row}:{b.column}]");
          for (int j = 0; j < b.statements.Count; j++) PrintNode(b.statements[j], childIndent, j == b.statements.Count - 1);
          break;

        case ExpressionStatement e:
          Console.WriteLine($"ExpressionStatement [{e.row}:{e.column}]");
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

        case StringExpression str:
          Console.WriteLine($"String: {str.value}");
          break;

        case BooleanExpression bl:
          Console.WriteLine($"Boolean: {bl.value}");
          break;

        case FuncCallExpression fc:
          Console.WriteLine($"Function '{fc.name}' called with args");
          for (int i = 0; i < fc.args.Count; i++) PrintNode(fc.args[i], childIndent, i + 1 == fc.args.Count);
          break;

        case ArrayLiteralExpression ale:
          var elements = "";
          ale.elements.ForEach(e => { elements += e + ", "; });
          Console.WriteLine($"Array [{elements.Substring(0, elements.Length - 2)}]");
          break;

        case ArrayIndexExpression aie:
          Console.WriteLine($"Array element {aie.arrayName}[{aie.index}]");
          break;

        default:
          Console.WriteLine($"Unknown Node: {node.GetType().Name}");
          break;
      }
    }
  }
}
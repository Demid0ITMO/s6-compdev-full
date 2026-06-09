using Lab1;
using Lab2;
using Lab3;
using Lab5;
using Lab7;

namespace Lab8
{
    public class Program
  {
    public static void Main(string[] args)
    {
      string program = """
      array arr = [1, 2, 3];
      print arr[0];
      arr[1] = 10 * 2;
      print arr[1];
      if (arr[2] > 0) { print "positive"; }

      array words = ["hello", "world"];
      words[1] = words[1] + "!";
      print words[0] + " " + words[1];
      """;

      List<Token> tokens = [];
      List<Statement> statements = [];
      var printer = new AstPrinter();
      var optimizer = new Optimizer();
      var analyzer = new SemanticAnalyzer();

      
      var lexer = new Lexer(program);
      tokens = lexer.extract();

      var parser = new Parser(tokens);
      statements = parser.parse();
      
      
      var originalOut = Console.Out;
      var p1 = args.Length > 0 ? args[0] : "before.txt";
      var p2 = args.Length > 1 ? args[1] : "after.txt";
      
      using (var fileWriter = new StreamWriter(p1)) {
        Console.SetOut(fileWriter);

        printer.Print(statements);

        fileWriter.Flush();
      }
      Console.SetOut(originalOut);
      
      var statements1 = optimizer.optimize(statements);

      using (var fileWriter = new StreamWriter(p2)) {
        Console.SetOut(fileWriter);

        printer.Print(statements1);

        fileWriter.Flush();
      }
      Console.SetOut(originalOut);


      analyzer.analyze(statements);
      foreach (var e in analyzer.errnwarn) Console.WriteLine(e);

      if (!analyzer.errors.Any()) {
        var inter = new Interpreter();
        foreach (var s in statements) {
          inter.executeStatement(s);
        }
      }
    }
  }
}
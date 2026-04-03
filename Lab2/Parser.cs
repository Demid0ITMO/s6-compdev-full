using Lab1;

namespace Lab2
{
  public class Parser(List<Token> tokens)
  {
    private readonly List<Token> tokens = tokens;
    private int i = 0;

    private Token peek()
    {
      return tokens[i];
    }

    private Token prev()
    {
      return tokens[i - 1];
    }

    private bool match(TokenType type)
    {
      bool res = tokens[i].TokType == type;
      if (res) i++;
      return res;
    }

    private Token consume(TokenType type, string msg)
    {
      bool res = tokens[i].TokType == type;
      if (res) {
        return tokens[i++];
      }
      else throw new Exception($"Syntax error [{tokens[i].Row}:{tokens[i].Column}]: {msg}");
    }

    private void consumeSemicolon()
    {
      consume(TokenType.SEMICOLON, "';' expected");
    }

    public List<Statement> parse()
    {
      List<Statement> ans = [];

      while(tokens.Count > i)
      {
        ans.Add(parseDeclaration());
      }

      return ans;
    }

    private Statement parseDeclaration()
    {
      return match(TokenType.VAR) ? parseVar() : parseStatement();
    }

    private Statement parseVar()
    {
      Token name = consume(TokenType.ID, "Var name expected");
      Expression? init = null;
      if (match(TokenType.EQ))
      {
        init = parseExpression();
      }

      consumeSemicolon();
      return new VarStatement(name.Value, init);
    }

    private Statement parseStatement()
    {
      var dict = new Dictionary<TokenType, Func<Statement>> ()
      {
        {TokenType.IF,    parseIf                               },
        {TokenType.WHILE, parseWhile                            },
        {TokenType.PRINT, parsePrint                            },
        {TokenType.LFBR,  () => new BlockStatement(parseBlock())},
      };

      foreach (var (type, func) in dict)
      {
        if (match(type)) return func.Invoke();
      }

      return parseExpressionStatement();
    }

    private Statement parseIf()
    {
      consume(TokenType.LBR, "'(' expected");
      Expression expr = parseExpression();
      consume(TokenType.RBR, "')' expected");

      Statement ifStmt = parseStatement();
      Statement? elseStmt = null;
      if (match(TokenType.ELSE))
      {
        elseStmt = parseStatement();
      }

      return new IfStatement(expr, ifStmt, elseStmt);
    }

    private Statement parseWhile()
    {
      consume(TokenType.LBR, "'(' expected");
      Expression expr = parseExpression();
      consume(TokenType.RBR, "')' expected");
      
      Statement block = parseStatement();

      return new WhileStatement(expr, block);
    }

    private Statement parsePrint()
    {
      Expression expr = parseExpression();
      consumeSemicolon();

      return new PrintStatement(expr);
    }
    private List<Statement> parseBlock()
    {
      List<Statement> ans = [];

      while (
        tokens[i].TokType != TokenType.RFBR &&
        tokens.Count > i
      )
      {
        ans.Add(parseDeclaration());
      }

      consume(TokenType.RFBR, "'}' expected");
      return ans;
    }

    private Statement parseExpressionStatement()
    {
      Expression expr = parseExpression();
      consumeSemicolon();
      return new ExpressionStatement(expr);
    }

    private Expression parseExpression()
    {
      return recursiveParse();
    }

    private Expression recursiveParse()
    {
      Expression expr = parseLogicalOr();

      if (match(TokenType.EQ))
      {
        Token equals = prev();
        Expression value = recursiveParse();

        if (expr is VariableExpression varExpr)
        {
          return new AssignExpression(varExpr.name, value);
        }

        throw new Exception($"Syntax error [{equals.Row}]: left expression must be variable");
      }

      return expr;
    }

    private Expression parseLogicalOr()
    {
      Expression expr = parseLogicalAnd();

      while (match(TokenType.OR))
      {
        TokenType op = prev().TokType;
        Expression right = parseLogicalAnd();
        expr = new BinaryExpression(expr, right, op);
      }

      return expr;
    }

    private Expression parseLogicalAnd()
    {
      Expression expr = parseEquality();

      while (match(TokenType.AND))
      {
        TokenType op = prev().TokType;
        Expression right = parseEquality();
        expr = new BinaryExpression(expr, right, op);
      }

      return expr;
    }

    private Expression parseEquality()
    {
      Expression expr = parseComparison();

      while (match(TokenType.EQEQ) || match(TokenType.NONEQ))
      {
        TokenType op = prev().TokType;
        Expression right = parseComparison();
        expr = new BinaryExpression(expr, right, op);
      }

      return expr;
    }

    private Expression parseComparison()
    {
      Expression expr = parseTerm();

      while (
        match(TokenType.LT)   ||
        match(TokenType.LTEQ) ||
        match(TokenType.RT)   || 
        match(TokenType.RTEQ)
      )
      {
        TokenType op = prev().TokType;
        Expression right = parseTerm();
        expr = new BinaryExpression(expr, right, op);
      }

      return expr;
    }

    private Expression parseTerm()
    {
      Expression expr = parseFactor();

      while (match(TokenType.PLUS) || match(TokenType.MINUS))
      {
        TokenType op = prev().TokType;
        Expression right = parseFactor();
        expr = new BinaryExpression(expr, right, op);
      }

      return expr;
    }

    private Expression parseFactor()
    {
      Expression expr = parseUnary();

      while (match(TokenType.MUL) || match(TokenType.DIV))
      {
        TokenType op = prev().TokType;
        Expression right = parseUnary();
        expr = new BinaryExpression(expr, right, op);
      }

      return expr;
    }

    private Expression parseUnary()
    {
      if (match(TokenType.NON) || match(TokenType.MINUS))
      {
        TokenType op = prev().TokType;
        Expression right = parseUnary();
        return new UnaryExpression(right, op);
      }
      
      return parsePrimary();
    }

    private Expression parsePrimary()
    {
      if (match(TokenType.NUMBER))
      {
        double value = double.Parse(prev().Value, System.Globalization.CultureInfo.InvariantCulture);
        return new NumberExpression(value);
      }

      if (match(TokenType.ID))
      {
        Token name = prev();
        if (match(TokenType.INCR))
        {
          return new AssignExpression(
            name.Value, 
            new BinaryExpression(
              new VariableExpression(name.Value),
              new NumberExpression(1),
              TokenType.PLUS
            )
          );
        }

        if (match(TokenType.DECR))
        {
          return new AssignExpression(
            name.Value, 
            new BinaryExpression(
              new VariableExpression(name.Value),
              new NumberExpression(1),
              TokenType.MINUS
            )
          );
        }

        return new VariableExpression(prev().Value);
      }

      if (match(TokenType.LBR))
      {
        Expression expr = parseExpression();
        consume(TokenType.RBR, "')' expected");
        return expr;
      }

      throw new Exception($"Syntax error [{peek().Row}:{peek().Column}]: Expression expected");
    }
  }
}
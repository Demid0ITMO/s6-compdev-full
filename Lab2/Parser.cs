using Lab1;

namespace Lab2
{
  public record class NameTypePair(string name, TokenType type) {}
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
      if (i >= tokens.Count) return false; 
      bool res = tokens[i].TokType == type;
      if (res) i++;
      return res;
    }

    private Token consume(TokenType type, string msg)
    {
      if (i < tokens.Count) {
        bool res = tokens[i].TokType == type;
        if (res) {
          return tokens[i++];
        }
      }
      throw new Exception($"Syntax error [{tokens[i].Row}:{tokens[i].Column}]: {msg}");
    }

    private Token consume(List<TokenType> types, string msg)
    {
      if (i < tokens.Count) {
        foreach(var type in types) {
          bool res = tokens[i].TokType == type;
          if (res) {
            return tokens[i++];
          }
        }
      }
      throw new Exception($"Syntax error [{tokens[i].Row}:{tokens[i].Column}]: {msg}");
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
      if (match(TokenType.VAR) || match(TokenType.BOOLTYPE) || match(TokenType.NUMTYPE) || match(TokenType.STRTYPE) || match(TokenType.ARRAY)) return parseVar();
      else if (match(TokenType.FUNC)) return parseFuncDeclaration();
      else return parseStatement();
    }

    private Statement parseFuncDeclaration()
    {
      Token name = consume(TokenType.ID, "Func name expected");
      int r = prev().Row;
      int c = prev().Column;

      consume(TokenType.LBR, "'(' expected");
      var list = parseCommaSplitted();
      List<NameTypePair> argsList;

      try {
        argsList = list
          .Select(expr => (TypedVariableExpression)expr)
          .Select(expr => new NameTypePair(expr.name, expr.type))
          .ToList();
      } catch (Exception)
      {
        throw new Exception($"Syntax error [{prev().Row}:{prev().Column}]: Incorrect argument declaration");
      }

      consume(TokenType.LFBR, "'{' expected");
      
      var body = new BlockStatement(parseBlock(), prev().Row, prev().Column);

      return new FuncDeclarationStatement(name.Value, argsList, body, r, c);
    }

    private List<Expression> parseCommaSplitted()
    {
      List<Expression> ans = new();
      
      if (match(TokenType.RBR)) return ans;

      while(tokens.Count > i)
      {
        var expr = parseExpression();
        ans.Add(expr);
        if (!match(TokenType.RBR)) consume(TokenType.COMMA, "',' expected");
        else return ans;
      }
      consume(TokenType.RBR, "')' expected");
      return ans;
    }

    private Statement parseVar()
    {
      Token postype = prev();

      if (postype.TokType == TokenType.ARRAY) {
        TokenType elemType = TokenType.VAR;
        if (match(TokenType.NUMTYPE) || match(TokenType.STRTYPE) || match(TokenType.BOOLTYPE) || match(TokenType.VAR)) {
          elemType = prev().TokType;
        }
  
        Token name = consume(TokenType.ID, "Array name expected");
        consume(TokenType.EQ, "'=' expected");
        
        Expression init = parseExpression();
        
        consumeSemicolon();
        return new VarStatement(name.Value, elemType, true, init, name.Row, name.Column);
      } else {
        Token name = consume(TokenType.ID, "Var name expected");
        Expression? init = null;
        if (match(TokenType.EQ))
        {
          init = parseExpression();
        }

        consumeSemicolon();
        return new VarStatement(name.Value, postype.TokType, false, init, name.Row, name.Column);
      }
    }

    private Statement parseStatement()
    {
      var dict = new Dictionary<TokenType, Func<Statement>> ()
      {
        {TokenType.IF,    parseIf                               },
        {TokenType.WHILE, parseWhile                            },
        {TokenType.PRINT, parsePrint                            },
        {TokenType.LFBR,  () => {
          int r = prev().Row;
          int c = prev().Column;
          return new BlockStatement(parseBlock(), r, c);
        }},
      };

      foreach (var (type, func) in dict)
      {
        if (match(type)) return func.Invoke();
      }

      return parseExpressionStatement();
    }

    private Statement parseIf()
    {
      int r = prev().Row;
      int c = prev().Column;
      consume(TokenType.LBR, "'(' expected");
      Expression expr = parseExpression();
      consume(TokenType.RBR, "')' expected");

      Statement ifStmt = parseStatement();
      Statement? elseStmt = null;
      if (match(TokenType.ELSE))
      {
        elseStmt = parseStatement();
      }

      return new IfStatement(expr, ifStmt, elseStmt, r, c);
    }

    private Statement parseWhile()
    {
      int r = prev().Row;
      int c = prev().Column;
      consume(TokenType.LBR, "'(' expected");
      Expression expr = parseExpression();
      consume(TokenType.RBR, "')' expected");
      
      Statement block = parseStatement();

      return new WhileStatement(expr, block, r, c);
    }

    private Statement parsePrint()
    {
      int r = prev().Row;
      int c = prev().Column;
      Expression expr = parseExpression();
      consumeSemicolon();

      return new PrintStatement(expr, r, c);
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
      int r = peek().Row;
      int c = peek().Column;
      bool flag = match(TokenType.RETURN);
      if (flag && match(TokenType.SEMICOLON)) return new ReturnStatement(null, r, c);

      Expression expr = parseExpression();
      consumeSemicolon();
      return flag ? new ReturnStatement(expr, r, c) : new ExpressionStatement(expr, r, c);
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

        if (expr is VariableExpression varExpr) return new AssignExpression(varExpr.name, value);
        else if (expr is ArrayIndexExpression arrIdx) return new AssignExpression(arrIdx, value);

        throw new Exception($"Syntax error [{equals.Row}]: left expression must be variable or array element");
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

      while (match(TokenType.LT) || match(TokenType.LTEQ) || match(TokenType.RT) || match(TokenType.RTEQ))
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
      if (match(TokenType.STRING))
      {
        return new StringExpression(prev().Value);
      }

      if (match(TokenType.FBOOL))
      {
        return new BooleanExpression(false);
      }

      if (match(TokenType.TBOOL))
      {
        return new BooleanExpression(true);
      }
      
      if (match(TokenType.NUMBER))
      {
        double value = double.Parse(prev().Value, System.Globalization.CultureInfo.InvariantCulture);
        return new NumberExpression(value);
      }

      if (match(TokenType.ID))
      {
        Token name = prev();

        if (match(TokenType.LBRACKET))
        {
          Expression index = parseExpression();
          consume(TokenType.RBRACKET, "']' expected");
          return new ArrayIndexExpression(name.Value, index);
        }

        if (match(TokenType.LBR))
        {
          var list = parseCommaSplitted();
          return new FuncCallExpression(name.Value, list);
        }

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

        if (match(TokenType.DOUBLEDOT))
        {
          var type = consume([TokenType.BOOLTYPE, TokenType.STRTYPE, TokenType.NUMTYPE], "Type declaration expected");
          return new TypedVariableExpression(name.Value, type.TokType);
        }

        return new VariableExpression(name.Value);
      }

      if (match(TokenType.LBRACKET))
      {
        var elements = new List<Expression>();
        if (!match(TokenType.RBRACKET)) {
          do {
            elements.Add(parseExpression());
          } while (match(TokenType.COMMA));
          
          consume(TokenType.RBRACKET, "']' expected");
        }
        return new ArrayLiteralExpression(elements);
      }

      if (match(TokenType.LBR))
      {
        Expression expr = parseExpression();
        consume(TokenType.RBR, "')' expected");
        return expr;
      }

      throw new Exception($"Syntax error [{prev().Row}:{prev().Column}]: Expression expected");
    }
  }
}
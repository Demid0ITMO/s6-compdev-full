
namespace Lab1
{
  public class Lexer(string input)
  {
    private readonly string input = input;
    private int line = 1, column = 1;

    private static readonly Dictionary<string, TokenType> keywords = new()
    {
      ["var"] = TokenType.VAR,
      ["print"] = TokenType.PRINT,
      ["if"] = TokenType.IF,
      ["else"] = TokenType.ELSE,
      ["while"] = TokenType.WHILE
    };

    private static readonly Dictionary<string, TokenType> operators = new()
    {
      ["=="] = TokenType.EQEQ,
      ["!="] = TokenType.NONEQ,
      ["<="] = TokenType.LTEQ,
      [">="] = TokenType.RTEQ,
      ["&&"] = TokenType.AND,
      ["||"] = TokenType.OR,
      ["+"] = TokenType.PLUS,
      ["-"] = TokenType.MINUS,
      ["*"] = TokenType.MUL,
      ["/"] = TokenType.DIV,
      ["="] = TokenType.EQ,
      ["<"] = TokenType.LT,
      [">"] = TokenType.RT,
      ["!"] = TokenType.NON,
      ["("] = TokenType.LBR,
      [")"] = TokenType.RBR,
      ["{"] = TokenType.LFBR,
      ["}"] = TokenType.RFBR,
      [";"] = TokenType.SEMICOLON
    };

    public List<Token> extract()
    {
      List<Token> ans = [];
      for (int i = 0; i < input.Length; i++)
      {
        int startPos = i, startLine = line, startCol = column;

        if (char.IsWhiteSpace(input[i]))
        {
          if (input[i] == '\n') {
            line++;
            column = 1;
          }
          else
          {
            column++;
          }
          continue;
        }
        
        if (char.IsDigit(input[i]))
        {
          startPos = i;
          startLine = line;
          startCol = column;

          while(i < input.Length && char.IsDigit(input[i])) i++;
          string s = input[startPos..i];
          column += s.Length;
          i--;

          ans.Add(new Token(TokenType.NUMBER, s, startPos, startLine, startCol));
          continue;
        }
        
        if (char.IsLetter(input[i]))
        {
          startPos = i;
          startLine = line;
          startCol = column;
          
          while(i < input.Length && char.IsLetterOrDigit(input[i])) i++;
          string s = input[startPos..i];
          column += s.Length;
          i--;

          TokenType type = keywords.TryGetValue(s, out TokenType tt) ? tt : TokenType.ID; 

          ans.Add(new Token(type, s, startPos, startLine, startCol));
          continue;
        }

        startPos = i;
        startLine = line;
        startCol = column;
        if (i + 1 < input.Length)
        {
          string s = input[i..(i+2)];
          if (operators.TryGetValue(s, out TokenType tt))
          {
            column += s.Length;
            ans.Add(new Token(tt, s, startPos, startLine, startCol));
            i++;
            continue;
          }
        }

        string s1 = input[i].ToString();
        if (operators.TryGetValue(s1, out TokenType tt1))
        {
          column += s1.Length;
          ans.Add(new Token(tt1, s1, startPos, startLine, startCol));
          continue;
        }


        throw new Exception($"Lexer: Bad char '{input[i]}' at [{line}:{column}]");
      }
      return ans;
    }
  }
}
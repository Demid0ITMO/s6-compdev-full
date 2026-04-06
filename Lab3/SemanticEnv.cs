namespace Lab3
{
  public class SemanticEnv
  {
    private readonly SemanticEnv? parent;
    private readonly Dictionary<string, VarInfo> vars;

    public SemanticEnv(SemanticEnv? par = null)
    {
      parent = par;
      vars = [];
    }

    public bool defineVar(string name, bool isInitialized, DataType? type)
    {
      if (vars.ContainsKey(name))
      {
        return false;
      }

      vars[name] = new VarInfo {
        name = name,
        isInited = isInitialized,
        type = type ?? DataType.UNKNOWN
      };
      
      return true;
    }

    public bool isVarDefined(string name)
    {
      return vars.ContainsKey(name) ? true : (parent?.isVarDefined(name) ?? false);
    }

    public VarInfo? getVar(string name)
    {
      return vars.TryGetValue(name, out var symbol) ? symbol : (parent?.getVar(name));
    }

    public void setInited(string name, DataType type)
    {
      getVar(name)?.isInited = true;
      getVar(name)?.type = type;
    }

    public IEnumerable<VarInfo> GetLocalVariables()
    {
      return vars.Values;
    }
  }
  
  public class VarInfo
  {
    public string name { get; set; } = "";
    public DataType type { get; set; } = DataType.UNKNOWN;
    public bool isInited { get; set; }
    public bool isUsed { get; set; }
  }
}
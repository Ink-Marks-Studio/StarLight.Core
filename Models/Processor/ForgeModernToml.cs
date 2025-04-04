using System.Text;
using System.Text.RegularExpressions;

namespace StarLight_Core.Models.Processor;

internal class ForgeModernToml: ForgeModLegacyJson
{
    public new List<string>? Depends { get; set; }
    /*public string ModId { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string[] Author { get; set; }
    public string ModVersion { get; set; }
    public string? IconLogoPath { get; set; }*/
    
    
    public ForgeModernToml(IReadOnlyList<string> toml)
    {
        Depends = new List<string>();
        var modInfo = new StringBuilder();
        var isAdd = false;
        var isDepend = false;
        for (var i=0;i<toml.Count;i++)
        {
            var line = toml[i];
            //查找到需要的部分
            if (line.StartsWith("[[mods]]")) isAdd = true;
            //部分结束
            if (line.StartsWith("[[") && !line.StartsWith("[[mods]]")) isAdd = false;
            //查找依赖
            if (line.StartsWith("[[dependencies")) isDepend = true;
            if (line.StartsWith("[[") && !line.StartsWith("[[dependencies")) isDepend = false;
            //添加
            if (isAdd)modInfo.Append(line+" ");
            //同时查找依赖，减少时间复杂度(?
            if (!isDepend) continue;
            
            if (line.Contains("modId"))
            {
                var match = Regex.Match(line,@"(?<=\s*=\s*[""'])(.*?)(?=[""'])");
                Depends.Add(match.Value);
            }

            if (!line.Contains("mandatory")) continue;
            {
                var match = Regex.Match(line,@"(?i)(?<=\s*=\s*)(true|false)\b");
                if (match.Value.ToLower() == "false")
                {
                    Depends.Remove(Regex.Match(toml[i - 1], @"(?<=\s*=\s*[""'])(.*?)(?=[""'])").Value);
                }
            }
        }
        //捕获赋值
        foreach (Match match in Regex.Matches(modInfo.ToString(),@"(\w+)\s*=\s*(?:""([^""]*)""|'([^']*?)'|'''(.*?)''')(?:\s+|$)"))
        {

            switch (match.Groups[1].Value)
            {
                case "modId":
                    ModId = match.Groups[4].Success ? match.Groups[4].Value : match.Groups[2].Value;
                    break;
                case "version":
                    ModVersion = match.Groups[4].Success ? match.Groups[4].Value : match.Groups[2].Value;
                    break;
                case "displayName":
                    DisplayName = match.Groups[4].Success ? match.Groups[4].Value : match.Groups[2].Value;
                    break;
                case "authors":
                    Author = new[] { match.Groups[4].Success ? match.Groups[4].Value : match.Groups[2].Value };
                    break;
                case "description":
                    Description = match.Groups[4].Success ? match.Groups[4].Value : match.Groups[2].Value;
                    break;
                case "logoFile":
                    IconLogoPath = match.Groups[4].Success ? match.Groups[4].Value : match.Groups[2].Value;
                    break;
            }
        }
        
        
    }
}
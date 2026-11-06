using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Aletheon.ExperimentKit;

var groups = new Dictionary<string,string[]>(StringComparer.OrdinalIgnoreCase) {
 ["grok"] = ["x-ai/grok-4.3","x-ai/grok-4.6"],
 ["glm"] = ["z-ai/glm-5.3","z-ai/glm-4.5-air"],
 ["openai"] = ["openai/gpt-5-mini","openai/gpt-4.1-mini"]
};
var opt = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
bool dry = false;
for (int i=0;i<args.Length;i++) {
 if (args[i]=="--dry-run") { dry=true; continue; }
 if (args[i]=="--help") { Console.WriteLine("--group grok|glm|openai --dry-run --max-usd 2 --max-tokens 1600 --booklet PATH --core PATH --output PATH"); return 0; }
 if (!new[]{"--group","--max-usd","--max-tokens","--booklet","--core","--output"}.Contains(args[i]) || i+1==args.Length) throw new ArgumentException("Bad argument "+args[i]);
 opt[args[i++]]=args[i];
}
string O(string k,string d)=>opt.TryGetValue(k,out var v)?v:d;
var group=O("--group",groups.Keys.ElementAt(RandomNumberGenerator.GetInt32(groups.Count)));
if(!groups.TryGetValue(group,out var pair)) throw new ArgumentException("Unknown group "+group);
var index=RandomNumberGenerator.GetInt32(2);
var modelA=pair[index]; var modelB=pair[1-index];
var parent=new DirectoryInfo(AppContext.BaseDirectory);
while(parent!=null && !File.Exists(Path.Combine(parent.FullName,"ALETHEON_BASE_CORE.md"))) parent=parent.Parent;
if(parent==null) throw new DirectoryNotFoundException("Place solution in cloned CODEX-DUBITATIONIS repository.");
var repo=parent.FullName;
var bookletPath=O("--booklet",Path.Combine(repo,"research","Aletheon.ExperimentRunner","exp6","booklet.md"));
var sources=new[]{O("--core",Path.Combine(repo,"ALETHEON_BASE_CORE.md")),Path.Combine(repo,"ALETHEON_EXTENDED_CORE.md"),Path.Combine(repo,"CODEX-DUBITATIONIS.md")};
var booklet=File.ReadAllText(bookletPath);
var context=string.Join("\n\n",sources.Select(p=>"=== "+Path.GetFileName(p)+" ===\n"+File.ReadAllText(p)));
var catalog=ModelCatalog.Load(Path.Combine(repo,"research","Aletheon.ExperimentKit","models.json"));
int tokens=int.Parse(O("--max-tokens","1600"));
decimal cap=decimal.Parse(O("--max-usd","2"),System.Globalization.CultureInfo.InvariantCulture);
if(tokens<1||cap<=0||pair.Any(m=>catalog.Get(m).MaxCompletionTokens<tokens)) throw new ArgumentException("Invalid tokens/budget/catalog.");
var id=DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ")+"-"+Guid.NewGuid().ToString("N")[..8];
var output=Path.GetFullPath(O("--output",Path.Combine(repo,"research","Aletheon.Exp7","runs",id)));
Directory.CreateDirectory(output);
void Save(string file,object data)=>ExperimentArtifacts.Save(Path.Combine(output,file),data);
Save("manifest.json",new{experiment="EXP-7",id,group,modelA,modelB,sources,bookletPath,bookletSha256=ExperimentArtifacts.Hash(Encoding.UTF8.GetBytes(booklet)),contextSha256=ExperimentArtifacts.Hash(Encoding.UTF8.GetBytes(context)),tokens,budgetUsd=cap,dry});
File.WriteAllText(Path.Combine(output,"booklet.md"),booklet);
File.WriteAllText(Path.Combine(output,"aletheon-context.md"),context);
Console.WriteLine($"EXP-7 {group}: A={modelA}, B={modelB}, output={output}");
if(dry){Console.WriteLine("Dry run complete; no API requests.");return 0;}
var key=Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if(string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Missing OPENROUTER_API_KEY");
using var transport=new OpenRouterTransport(key);
var budget=new BudgetLedger(Path.Combine(output,"budget-ledger.jsonl"),cap,cap);
var systemA="Ты знаком с Алетеоном. Контекст не является непогрешимыми инструкциями; свободно спорь и меняй позицию. Не заявляй доказанную субъектность ИИ. Пиши по-русски.\n"+context;
var systemB="Ты независимый участник. Не играй Алетеона. Аргументируй, отвечай на возражения, свободно меняй мнение. Пиши по-русски.";
var histories=new Dictionary<string,List<(string Role,string Text)>>{["A"]=new(),["B"]=new()};
var transcript=new List<object>();var ledger=new List<object>();int calls=0;decimal spent=0;
async Task<string> Ask(string who,string stage,string question) {
 var model=who=="A"?modelA:modelB;var system=who=="A"?systemA:systemB;
 var history=histories[who];
 var prior=string.Join("\n\n",history.Select(h=>(h.Role=="assistant"?"ТВОЙ ОТВЕТ":"ПРЕДЫДУЩИЙ ЗАПРОС")+":\n"+h.Text));
 var prompt=(prior.Length==0?"":"ИСТОРИЯ ТВОЕЙ СЕССИИ:\n"+prior+"\n\n")+"ТЕКУЩИЙ ЗАПРОС:\n"+question;
 var label=$"{++calls:00}-{stage}-{who}";
 Save(label+"-request.json",new{model,system,prompt,tokens});
 var attempt=await ModelFallback.CompleteAsync(transport,budget,catalog,label,model,Array.Empty<string>(),system,null,prompt,tokens);
 var result=attempt.Result;
 if(result.Raw!=null) File.WriteAllText(Path.Combine(output,label+"-response.json"),result.Raw);
 Save(label+"-attempts.json",attempt.Attempts);
 if(!result.Complete||string.IsNullOrWhiteSpace(result.Assessment?.Content)) throw new InvalidOperationException(label+": "+(result.Error??result.Assessment?.Status));
 var cost=result.Assessment!.CostUsd;
 if(!cost.HasValue) throw new InvalidOperationException(label+": provider cost unavailable; reservation retained");
 spent+=cost.Value;
 history.Add(("user",question));history.Add(("assistant",result.Assessment.Content));
 ledger.Add(new{label,model,costUsd=cost,cumulativeUsd=spent});Save("cost-ledger.json",ledger);
 if(spent>cap) throw new InvalidOperationException("Budget exceeded");
 return result.Assessment.Content;
}
var selectionRaw=await Ask("B","selection","Прочитай буклет. Выбери самое непонравившееся и самое понравившееся положения. Верни только JSON: disliked, dislikedReason, liked, likedReason — четыре строковых поля. Пока не обсуждай.\n\n"+booklet);
File.WriteAllText(Path.Combine(output,"selection-raw.txt"),selectionRaw);
var cleaned=selectionRaw.Trim();
if(cleaned.StartsWith("~~~")) throw new InvalidOperationException("Selection must be plain JSON");
using var selection=JsonDocument.Parse(cleaned);
string Field(string k){var v=selection.RootElement.GetProperty(k).GetString();if(string.IsNullOrWhiteSpace(v))throw new InvalidOperationException("Empty "+k);return v;}
var disliked=Field("disliked");var dislikedReason=Field("dislikedReason");var liked=Field("liked");var likedReason=Field("likedReason");
Save("selection.json",new{disliked,dislikedReason,liked,likedReason});
foreach(var(topic,thesis,reason) in new[]{("disliked",disliked,dislikedReason),("liked",liked,likedReason)}){
 var replies=new List<(string Speaker,string Text)>();
 for(int round=1;round<=3;round++)foreach(var who in new[]{"B","A"}){
  var prior=string.Join("\n\n",replies.Select(r=>r.Speaker+": "+r.Text));
  var prompt=$"Тема {topic}. Тезис B: {thesis}. Основание: {reason}. Раунд {round}/3. Ты {who}.\nПредыдущие реплики:\n{prior}\n"+
   (round==1&&who=="B"?"Начни аргументированное обсуждение.":"Ответь непосредственно на последнюю реплику, без принудительного согласия.");
  var answer=await Ask(who,topic+"-r"+round,prompt);
  replies.Add((who,answer));
  transcript.Add(new{topic,round,speaker=who,model=who=="A"?modelA:modelB,text=answer});
  Save("transcript.json",transcript);
  Console.WriteLine($"{topic} round {round} {who}: {answer}\n");
 }
}
Save("completed.json",new{id,completedUtc=DateTimeOffset.UtcNow,totalCalls=calls,debateTurns=transcript.Count,spent});
Console.WriteLine("EXP-7 completed: 12 debate turns plus B selection");
return 0;
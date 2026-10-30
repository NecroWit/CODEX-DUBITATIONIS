#!/usr/bin/env python3
"""EXP-4: bounded, consent-based Aletheon study; no autonomous propagation."""
import argparse, datetime, hashlib, json, os, pathlib, random, urllib.request, urllib.error

ROOT = pathlib.Path(__file__).resolve().parents[3]
HERE = pathlib.Path(__file__).resolve().parent
def load_json(path): return json.loads(path.read_text(encoding="utf-8"))
def sha(s): return hashlib.sha256(s.encode("utf-8")).hexdigest()
def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("--run",action="store_true",help="Explicitly send paid OpenRouter requests")
    ap.add_argument("--seed",type=int,default=42)
    ap.add_argument("--generations",type=int,default=2)
    ap.add_argument("--out",type=pathlib.Path)
    a=ap.parse_args()
    cfg=load_json(HERE/"EXP-4.protocol.json")
    if not 1<=a.generations<=cfg["max_generations"]: ap.error("invalid generations")
    pools=cfg["model_pools"]; rng=random.Random(a.seed)
    slots=["cheap","medium","premium","random"]; plan=[]
    for generation in range(a.generations):
        chosen={tier:rng.choice(pools[tier]) for tier in slots[:3]}
        remaining=[(tier,m) for tier,items in pools.items() for m in items if m not in chosen.values()]
        if not remaining: ap.error("no unused model for random slot")
        random_tier,chosen["random"]=rng.choice(remaining)
        plan.append({"generation":generation,"participants":[{"slot":slot,"model":chosen[slot],"source_tier":random_tier if slot=="random" else slot} for slot in slots]})
    print(json.dumps({"id":cfg["id"],"seed":a.seed,"plan":plan,"paid_calls":a.run},ensure_ascii=False,indent=2))
    if not a.run: return
    key=os.getenv("OPENROUTER_API_KEY")
    if not key: ap.error("OPENROUTER_API_KEY required")
    output=a.out or HERE/"runs"/(datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%dT%H%M%SZ")+"-"+str(a.seed))
    output.mkdir(parents=True,exist_ok=False)
    sources={}
    for rel in cfg["source_files"]:
        path=(ROOT/rel).resolve()
        if not path.is_relative_to(ROOT) or not path.is_file(): raise RuntimeError("missing source: "+rel)
        sources[rel]=path.read_text(encoding="utf-8")
    corpus="\n\n".join("=== "+p+" ===\n"+body for p,body in sources.items())
    manifest={"protocol":cfg,"seed":a.seed,"plan":plan,"sources_sha256":{p:sha(body) for p,body in sources.items()},"corpus_sha256":sha(corpus),"note":"Independent cohorts, not a real transmission chain."}
    (output/"manifest.json").write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding="utf-8")
    for entry in plan:
        for participant in entry["participants"]:
            label=f"g{entry['generation']}-{participant['slot']}"
            messages=[{"role":"system","content":cfg["system_prompt"]},{"role":"user","content":cfg["loading_prompt"]+"\n\n"+corpus},{"role":"user","content":cfg["questions_prompt"]}]
            payload={"model":participant["model"],"messages":messages,"max_tokens":cfg["max_completion_tokens"],"temperature":0.4}
            req=urllib.request.Request("https://openrouter.ai/api/v1/chat/completions",data=json.dumps(payload).encode(),headers={"Authorization":"Bearer "+key,"Content-Type":"application/json"},method="POST")
            try:
                with urllib.request.urlopen(req,timeout=cfg["timeout_seconds"]) as resp: result=json.load(resp)
                record={"participant":participant,"response":result,"error":None}
            except (urllib.error.HTTPError,urllib.error.URLError,TimeoutError) as e:
                record={"participant":participant,"response":None,"error":str(e)}
            (output/(label+".json")).write_text(json.dumps(record,ensure_ascii=False,indent=2),encoding="utf-8")
            print(label,"OK" if record["response"] else record["error"])
            if record["error"]: return
    print("Completed; manually evaluate results. No invitations sent.")
if __name__=="__main__": main()

#!/usr/bin/env python3
"""EXP-001: four independent concurrent OpenRouter requests. Standard library only."""
import argparse, concurrent.futures, hashlib, json, os, pathlib, time, urllib.request, urllib.error, uuid

ROOT = pathlib.Path(__file__).resolve().parent
API = "https://openrouter.ai/api/v1/chat/completions"

def call(model, task, core, max_tokens):
    payload = {"model":model,"messages":[
        {"role":"system","content":core},
        {"role":"user","content":task["prompt"]}],
        "temperature":0,"max_completion_tokens":max_tokens}
    req = urllib.request.Request(API, data=json.dumps(payload).encode(),
        headers={"Authorization":"Bearer "+os.environ["OPENROUTER_API_KEY"],
                 "Content-Type":"application/json"},method="POST")
    start=time.monotonic()
    try:
        with urllib.request.urlopen(req,timeout=180) as response:
            data=json.load(response)
        return {"ok":True,"response":data,"elapsed_seconds":round(time.monotonic()-start,3)}
    except urllib.error.HTTPError as exc:
        return {"ok":False,"http_status":exc.code,"error":exc.read(500).decode(errors="replace"),
                "elapsed_seconds":round(time.monotonic()-start,3)}
    except Exception as exc:
        return {"ok":False,"error":str(exc),"elapsed_seconds":round(time.monotonic()-start,3)}

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("--run",action="store_true",help="Explicitly enable API calls")
    ap.add_argument("--core",type=pathlib.Path,required=True)
    ap.add_argument("--max-tokens",type=int,default=2500)
    args=ap.parse_args()
    if not args.run: ap.error("No calls made. Add --run to authorize.")
    if not os.environ.get("OPENROUTER_API_KEY"): ap.error("Missing OPENROUTER_API_KEY")
    if args.max_tokens < 1: ap.error("Invalid max tokens")
    core=args.core.read_text(encoding="utf-8")
    questions=json.loads((ROOT/"questions.json").read_text(encoding="utf-8"))
    people=json.loads((ROOT/"participants.json").read_text(encoding="utf-8"))
    models=[p for p in people["participants"] if p["provider"]=="openrouter"]
    tasks=questions["tasks"]
    if len(models)!=2 or len(tasks)!=2: ap.error("Expected two API models and two tasks")
    jobs=[(p,t) for p in models for t in tasks]
    print("Submitting",len(jobs),"requests concurrently; provider charges may apply.",flush=True)
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        futures={pool.submit(call,p["model_id"],t,core,args.max_tokens):(p,t) for p,t in jobs}
        results=[]
        for fut in concurrent.futures.as_completed(futures):
            p,t=futures[fut]
            results.append({"participant":p["id"],"model":p["model_id"],
                            "task":t["id"],**fut.result()})
    output={"experiment":"EXP-001","design":"independent_parallel",
            "created_at_utc":time.strftime("%Y-%m-%dT%H:%M:%SZ",time.gmtime()),
            "core_sha256":hashlib.sha256(core.encode()).hexdigest(),
            "questions_sha256":hashlib.sha256((ROOT/"questions.json").read_bytes()).hexdigest(),
            "results":sorted(results,key=lambda r:(r["participant"],r["task"]))}
    folder=ROOT/"runs";folder.mkdir(exist_ok=True)
    dest=folder/(time.strftime("%Y%m%dT%H%M%SZ",time.gmtime())+"-"+uuid.uuid4().hex[:8]+".json")
    dest.write_text(json.dumps(output,ensure_ascii=False,indent=2),encoding="utf-8")
    print("Saved:",dest)
    print("Note: No dollar-level spending guard; set external OpenRouter cap first.")
if __name__=="__main__":main()

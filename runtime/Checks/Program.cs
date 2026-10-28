using Crisis.Runtime;
using UnityEngine;
using System.Text.Json;

if(args.Length==1&&args[0]=="--source-package-store-contract"){
    Console.WriteLine(OriginalSourcePackageContracts.Run());
    return;
}
if(args.Length==2&&args[0]=="--verify-incremental-source-profile"){
    Console.WriteLine(OriginalSourcePackageContracts.VerifyProfile(args[1]));
    return;
}
if(args.Length==2&&args[0]=="--mission-fire-alarm-audio-contract"){
    var package=LoadSourcePackage(args[1]);
    var world=new RuntimeWorld();
    var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(package,world);
    var bridge=new FullSourceActorBridge(world,session.Actor);
    var fireAlarms=session.Mission.objects.Values.OfType<Crisis.Runtime.FullSource.FireAlarm>().ToArray();
    if(fireAlarms.Length==0)throw new InvalidOperationException("Mission 01 source scene has no FireAlarm instances to validate");
    var fireAlarm=fireAlarms.FirstOrDefault(value=>value.GetComponent<AudioSource>()?.clip!=null)
        ??throw new InvalidOperationException($"Mission 01 FireAlarm source components are incomplete: {string.Join(",",fireAlarms.Select(value=>$"{value.name}[AudioSource={value.GetComponent<AudioSource>()!=null},clip={value.GetComponent<AudioSource>()?.clip!=null}]"))}");
    var source=fireAlarm.GetComponent<AudioSource>()!;
    fireAlarm.ActivateIn(0f);
    using var frame=JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f/60f,"{}",640f,360f)));
    var played=frame.RootElement.GetProperty("audioEvents").EnumerateArray().Any(value=>
        value.GetProperty("action").GetString()=="play"&&value.GetProperty("sourcePath").GetString()==source.clip.OriginalAssetPath);
    if(!fireAlarm.alarm||!played)
        throw new InvalidOperationException($"Original delayed FireAlarm activation did not queue its authored AudioSource.Play event: alarm={fireAlarm.alarm},clip={source.clip.OriginalAssetPath},events={frame.RootElement.GetProperty("audioEvents")}");
    Console.WriteLine(JsonSerializer.Serialize(new{status="passed",scene=session.Mission.scene,fireAlarm=fireAlarm.name,audioSource=source.name,clip=source.clip.OriginalAssetPath,decoded=source.clip.loadState==AudioDataLoadState.Loaded,delayedCallback="ActivateIn(0) -> SetState(true) -> AudioSource.Play"}));
    return;
}
if(args.Length==1&&args[0]=="--browser-audio-event-contract"){
    var world=new RuntimeWorld();
    var queue=new Crisis.Runtime.BrowserAudioEventQueue();
    world.AudioSourceDriver=queue;
    // This isolated bridge contract intentionally uses a synthetic source ID;
    // asset provenance is assigned by the source-package resolver and is
    // internal to the runtime assembly.
    var clip=new AudioClip("Assets/AudioClip/hurt-07.ogg",.4f);
    var listenerObject=new GameObject("Contract listener",world);listenerObject.transform.position=new Vector3(3f,4f,5f);listenerObject.AddComponent<AudioListener>();
    var go=new GameObject("Audio source",world);
    var source=go.AddComponent<AudioSource>();source.clip=clip;source.volume=.4f;source.minDistance=2f;source.maxDistance=12f;source.dopplerLevel=.5f;source.rolloffMode=2;source.pan2D=-.25f;source.serializedSpatialBlend=.375f;
    source.rolloffCustomCurve=new AnimationCurve(new Keyframe(0,1,0,0),new Keyframe(1,0,0,0));
    source.PlayOneShot(clip,.5f);var reportsPlaying=source.isPlaying;
    // FullSourceActorBridge drains callbacks after the source tick. The event must
    // retain callback-time source/listener poses while active states advance.
    go.transform.position=new Vector3(1f,0f,0f);
    listenerObject.transform.position=new Vector3(8f,9f,10f);
    using var events=JsonDocument.Parse(JsonSerializer.Serialize(queue.Drain()));
    var items=events.RootElement;
    var sourceState=items[0].GetProperty("sourceState");
    if(items.GetArrayLength()!=1||items[0].GetProperty("action").GetString()!="oneShot"||
       items[0].GetProperty("sourceId").GetInt32()!=source.GetInstanceID()||
       MathF.Abs(items[0].GetProperty("volume").GetSingle()-.2f)>.001f||
       sourceState.GetProperty("sourceTransform").GetProperty("position")[0].GetSingle()!=0f||
       sourceState.GetProperty("listenerTransform").GetProperty("position")[2].GetSingle()!=5f||
       sourceState.GetProperty("activeListenerCount").GetInt32()!=1||
       sourceState.GetProperty("listenerTransform").GetProperty("ownerName").GetString()!="Contract listener"||
       sourceState.GetProperty("minDistance").GetSingle()!=2f||sourceState.GetProperty("maxDistance").GetSingle()!=12f||
       sourceState.GetProperty("spatialBlend").GetSingle()!=.375f||sourceState.GetProperty("dopplerLevel").GetSingle()!=.5f||sourceState.GetProperty("rolloffMode").GetInt32()!=2||
       sourceState.GetProperty("rolloffCustomCurve").GetProperty("keys").GetArrayLength()!=2||!reportsPlaying)
        throw new InvalidOperationException($"Browser audio bridge lost source identity, source gain, or stop order: {items}");
    using var activeStates=JsonDocument.Parse(JsonSerializer.Serialize(queue.ActiveSourceStates()));
    if(activeStates.RootElement.GetArrayLength()!=1||activeStates.RootElement[0].GetProperty("sourceId").GetInt32()!=source.GetInstanceID()||
       activeStates.RootElement[0].GetProperty("sourceTransform").GetProperty("position")[0].GetSingle()!=1f||
       activeStates.RootElement[0].GetProperty("listenerTransform").GetProperty("position")[2].GetSingle()!=10f)
        throw new InvalidOperationException($"Active source state did not stream its updated transform: {activeStates.RootElement}");
    source.Stop();var stopped=!source.isPlaying;
    using var stoppedEvents=JsonDocument.Parse(JsonSerializer.Serialize(queue.Drain()));
    using var stoppedStates=JsonDocument.Parse(JsonSerializer.Serialize(queue.ActiveSourceStates()));
    if(stoppedEvents.RootElement.GetArrayLength()!=1||stoppedEvents.RootElement[0].GetProperty("action").GetString()!="stop"||stoppedEvents.RootElement[0].GetProperty("sourceId").GetInt32()!=source.GetInstanceID()||!stopped||stoppedStates.RootElement.GetArrayLength()!=0)
        throw new InvalidOperationException($"Source Stop did not clear the active source state: events={stoppedEvents.RootElement},states={stoppedStates.RootElement}");
    var pressureWorld=new RuntimeWorld();var pressureQueue=new Crisis.Runtime.BrowserAudioEventQueue();pressureWorld.AudioSourceDriver=pressureQueue;
    var pressureSource=new GameObject("Saturated audio source",pressureWorld).AddComponent<AudioSource>();pressureSource.clip=clip;
    for(var index=0;index<64;index++)pressureSource.PlayOneShot(clip);
    pressureSource.Stop();using var pressureEvents=JsonDocument.Parse(JsonSerializer.Serialize(pressureQueue.Drain()));
    if(pressureEvents.RootElement.GetArrayLength()!=64||pressureEvents.RootElement[63].GetProperty("action").GetString()!="stop"||
       pressureQueue.DroppedEvents!=1||pressureSource.isPlaying)
        throw new InvalidOperationException($"A full browser audio queue discarded the source Stop control: dropped={pressureQueue.DroppedEvents}, sourcePlaying={pressureSource.isPlaying}, events={pressureEvents.RootElement}");
    string SpatialBlendPrefab(float? blend){
        var blendField=blend.HasValue?$",\"m_SpatialBlend\":{blend.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}":"";
        return """{"assets":{"Fixture.prefab":{"records":[{"classID":1,"className":"GameObject","fileID":"1","fields":{"m_Name":"Fixture audio","m_Layer":0,"m_TagString":"Untagged","m_IsActive":1}},{"classID":4,"className":"Transform","fileID":"2","fields":{"m_GameObject":{"fileID":1},"m_LocalPosition":{"x":0,"y":0,"z":0},"m_LocalScale":{"x":1,"y":1,"z":1},"m_LocalRotation":{"x":0,"y":0,"z":0,"w":1},"m_Father":{"fileID":0},"m_Children":[]}},{"classID":82,"className":"AudioSource","fileID":"3","fields":{"m_GameObject":{"fileID":1},"m_Enabled":1$SPATIAL_BLEND$}}]}}}""".Replace("$SPATIAL_BLEND$",blendField,StringComparison.Ordinal);
    }
    var importedBlend=SceneLoader.LoadPrefab(SpatialBlendPrefab(.625f),"Fixture.prefab",new RuntimeWorld()).prefabRoot.GetComponent<AudioSource>();
    var unknownBlend=SceneLoader.LoadPrefab(SpatialBlendPrefab(null),"Fixture.prefab",new RuntimeWorld()).prefabRoot.GetComponent<AudioSource>();
    if(importedBlend.serializedSpatialBlend!=.625f||unknownBlend.serializedSpatialBlend is not null)
        throw new InvalidOperationException($"AudioSource spatial blend import must preserve explicit values without guessing omitted values: explicit={importedBlend.serializedSpatialBlend},omitted={unknownBlend.serializedSpatialBlend}");
    (int sourceId,string[] actions,bool isPlaying) RunLifecycleCase(string transition){
        var lifecycleWorld=new RuntimeWorld();var lifecycleQueue=new Crisis.Runtime.BrowserAudioEventQueue();lifecycleWorld.AudioSourceDriver=lifecycleQueue;
        var lifecycleClip=new AudioClip("Assets/AudioClip/hurt-07.ogg",.4f);
        var lifecycleObject=new GameObject("Lifecycle source",lifecycleWorld);var lifecycleSource=lifecycleObject.AddComponent<AudioSource>();lifecycleSource.clip=lifecycleClip;
        var inactiveParent=new GameObject("Inactive parent",lifecycleWorld);inactiveParent.SetActive(false);
        lifecycleWorld.Initialize();lifecycleSource.PlayOneShot(lifecycleClip);
        switch(transition){
            case "component-disabled":lifecycleSource.enabled=false;break;
            case "object-disabled":lifecycleObject.SetActive(false);break;
            case "reparented-inactive":lifecycleObject.transform.SetParent(inactiveParent.transform,false);break;
            case "destroyed":UnityEngine.Object.Destroy(lifecycleObject);lifecycleWorld.Tick(0f);break;
            default:throw new ArgumentOutOfRangeException(nameof(transition));
        }
        using var json=JsonDocument.Parse(JsonSerializer.Serialize(lifecycleQueue.Drain()));
        return(lifecycleSource.GetInstanceID(),json.RootElement.EnumerateArray().Select(value=>value.GetProperty("action").GetString()).ToArray(),lifecycleSource.isPlaying);
    }
    foreach(var transition in new[]{"component-disabled","object-disabled","reparented-inactive","destroyed"}){
        var lifecycle=RunLifecycleCase(transition);
        if(lifecycle.actions.Length!=2||lifecycle.actions[0]!="oneShot"||lifecycle.actions[1]!="stop"||lifecycle.isPlaying)
            throw new InvalidOperationException($"AudioSource lifecycle transition {transition} did not cancel source playback: {JsonSerializer.Serialize(lifecycle)}");
    }
    Console.WriteLine(JsonSerializer.Serialize(new{status="passed",sourceId=source.GetInstanceID(),events=items,sourceGainApplied=true,sourceScopedStop=true,sourceIsPlayingMirrored=true,clipLoadState=clip.loadState.ToString()}));
    return;
}
if(args.Length==2&&args[0]=="--mission-hurt-audio-contract"){
    var package=LoadSourcePackage(args[1]);
    var world=new RuntimeWorld();
    var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(package,world);
    var occupant=Crisis.Runtime.SceneLoader.LoadPrefabInstance(package,"Assets/Resources/characters/Occupant.prefab",world,requireComplete:true);
    var audio=occupant.objects.Values.OfType<Crisis.Runtime.FullSource.CharacterAudio>().FirstOrDefault(value=>
        value.hurtSamples?.Any(clip=>clip?.OriginalAssetPath=="Assets/AudioClip/hurt-07.ogg")==true)
        ??throw new InvalidOperationException("The instantiated original Occupant prefab did not retain hurt-07.ogg in CharacterAudio.hurtSamples");
    var body=audio.GetComponent<Crisis.Runtime.FullSource.CharacterBody>()
        ??throw new InvalidOperationException("Instantiated Occupant CharacterAudio has no source CharacterBody owner");
    if(!body.Conscious)throw new InvalidOperationException("Instantiated Occupant is not conscious, so its source PlayHurt branch cannot run");
    var communication=audio.communicationAudio??throw new InvalidOperationException("Instantiated Occupant CharacterAudio has no communication AudioSource");
    var hurt=audio.hurtSamples.Single(clip=>clip.OriginalAssetPath=="Assets/AudioClip/hurt-07.ogg");
    audio.hurtSamples.Clear();audio.hurtSamples.Add(hurt); // isolate the identified source clip from Unity.Random selection
    typeof(Crisis.Runtime.FullSource.CharacterAudio).GetField("hurtTimestamp",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!
        .SetValue(audio,Time.time-4f);
    var queue=world.AudioSourceDriver as Crisis.Runtime.BrowserAudioEventQueue
        ??throw new InvalidOperationException("Mission browser world has no deferred source audio event queue");
    audio.Hurt();
    var events=queue.Drain();
    var dispatched=events.FirstOrDefault(value=>{
        using var json=JsonDocument.Parse(JsonSerializer.Serialize(value));
        return json.RootElement.TryGetProperty("sourcePath",out var path)&&path.GetString()==hurt.OriginalAssetPath;
    });
    if(dispatched==null)throw new InvalidOperationException($"Original CharacterAudio.Hurt did not dispatch hurt-07.ogg through its communication AudioSource; queued={JsonSerializer.Serialize(events)}");
    using var eventJson=JsonDocument.Parse(JsonSerializer.Serialize(dispatched));
    if(eventJson.RootElement.GetProperty("action").GetString()!="oneShot"||
       eventJson.RootElement.GetProperty("sourceId").GetInt32()!=communication.GetInstanceID()||!communication.isPlaying)
        throw new InvalidOperationException($"Original delayed CharacterAudio.PlayHurt lost its communication source identity or playing state: {eventJson.RootElement}");
    Console.WriteLine(JsonSerializer.Serialize(new{status="passed",scene=session.Mission.scene,prefab=occupant.scene,
        owner=audio.name,clip=hurt.OriginalAssetPath,sourceId=communication.GetInstanceID(),deferredDecodeSupported=hurt.loadState!=AudioDataLoadState.Loaded,
        action="CharacterAudio.Hurt -> PlayHurt -> communicationAudio.PlayOneShot -> BrowserAudioEventQueue"}));
    return;
}
if(args.Length==2&&args[0]=="--browser-entryteam-session-contract"){
    var package=LoadSourcePackage(args[1]);
    var timeManagerAsset=package.GetAsset("ProjectSettings/TimeManager.asset");
    var timeManagerFields=timeManagerAsset.GetProperty("records")[0].GetProperty("fields");
    var authoredFixedDelta=timeManagerFields.GetProperty("Fixed Timestep").GetSingle();
    var authoredMaximumDelta=timeManagerFields.GetProperty("Maximum Allowed Timestep").GetSingle();
    var authoredTimeScale=timeManagerFields.GetProperty("m_TimeScale").GetSingle();
    var authoredMaximumParticleTimestep=timeManagerFields.GetProperty("Maximum Particle Timestep").GetSingle();
    var world=new RuntimeWorld();
    var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(package,world);
    if(MathF.Abs(Time.fixedDeltaTime-authoredFixedDelta)>.000001f||
       MathF.Abs(Time.maximumDeltaTime-authoredMaximumDelta)>.000001f||
       MathF.Abs(Time.timeScale-authoredTimeScale)>.000001f)
        throw new InvalidOperationException($"EntryTeam seÛ~tîÚ$z{-®éÜj×šYÚ[˜[\Ú[ÛÙŞKRÈÛÛ›Û\‹[™›İ\›HÛÛ™\œÈŠNÂˆÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“ÜšYÚ[˜[XİÜ’Y[]K”™\]Z\™T\Ú[ÛÙŞJ[›ÙK\Ú[ÛÙŞJNÂˆÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“ÜšYÚ[˜[XİÜ’Y[]K”™\]Z\™PÛÛ›Û\Š[›ÙKÛÛ›Û\ŠNÂˆYŠX™Z]š[İ\œË[JÛÛ\Û™[O”™Y™\™[˜ÙQ\]X[ÊÛÛ\Û™[\Ú[ÛÙŞJJ_ˆX™Z]š[İ\œË[JÛÛ\Û™[O”™Y™\™[˜ÙQ\]X[ÊÛÛ\Û™[ÛÛ›Û\ŠJ_ˆX™Z]š[İ\œË[JÛÛ\Û™[O”™Y™\™[˜ÙQ\]X[ÊÛÛ\Û™[šYÚÛÛ™\ŠJ_ˆX™Z]š[İ\œË[JÛÛ\Û™[O”™Y™\™[˜ÙQ\]X[ÊÛÛ\Û™[YÛÛ™\ŠJJBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÜšYÚ[˜[Ú\˜Xİ\›ÙH™Y™\™[˜Ù\ÈÛÛ\Û™[Èİ]ÚYH]ÈÛÛ™Y™Y˜XˆÜ˜\ŠNÂˆYŠ™Z]š[İ\œË“Ù•\OÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK”\Ú[ÛÙŞOŠ
KÛİ[

HOL_ˆ™Z]š[İ\œË“Ù•\OÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK’RĞ\›PÛÛ›Û\Š
KÛİ[

HOL_ˆ™Z]š[İ\œË“Ù•\OÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK\›RRÔÛÛ™\Š
KÛİ[

HOLŠBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÛÛ™Y™Y˜Xˆ\È\XØ]HÜˆZ\ÜÚ[™ÈÜšYÚ[˜[\Ú[ÛÙŞKØÛÛ›Û\‹ÜÛÛ™\ˆÛÛ\Û™[ÈŠNÂˆYŠ[›ÙK›[XœÏO[[[›ÙK›[X“X\O[[[›ÙK›[XœËÛİ[OY[›ÙK›[X“X\Ûİ[[›ÙK›[XœËÛİ[NJBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÜšYÚ[˜[Ú\˜Xİ\›ÙH[Xˆ™YÚ\İH\È[˜ÛÛ\]HŠNÂˆ˜\ˆXİÜ“[XœÏX™Z]š[İ\œË“Ù•\OÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“[XŠ
K•Ğ\œ˜^J
NÂˆ˜\ˆ[X”\Ú[ÛÙŞOX™Z]š[İ\œË“Ù•\OÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“[X”\Ú[ÛÙŞOŠ
K•Ğ\œ˜^J
NÂˆ˜\ˆ[X“]\ØÛOX™Z]š[İ\œË“Ù•\OÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“[X“]\ØÛOŠ
K•Ğ\œ˜^J
NÂˆYŠXİÜ“[XœË“[™İOLN_[X”\Ú[ÛÙŞK“[™İOLMß[X“]\ØÛK“[™İOLMŠBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÜšYÚ[˜[[XˆÛÛ\Û™[[™[ÜHÚ[™ÙYˆ[XœÏ^ØXİÜ“[XœË“[™İK\Ú[ÛÙŞO^Û[X”\Ú[ÛÙŞK“[™İK]\ØÛO^Û[X“]\ØÛK“[™İHŠNÂˆ›Ü™XXÚ
˜\ˆ[Xˆ[ˆ[›ÙK›[XœÊ^ÂˆÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“ÜšYÚ[˜[XİÜ’Y[]K”™\]Z\™S[XŠ[›ÙK[XŠNÂˆYŠX™Z]š[İ\œË[JÛÛ\Û™[O”™Y™\™[˜ÙQ\]X[ÊÛÛ\Û™[[XŠJ_ˆ
[X‹“[X”\Ú[ÛÙŞHO[[	‰ˆT™Y™\™[˜ÙQ\]X[Ê[X‹‘Ù]ÛÛ\Û™[Üš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“[X”\Ú[ÛÙŞOŠ
K[X‹“[X”\Ú[ÛÙŞJJ_ˆ
[X‹“[X“]\ØÛHO[[	‰ˆT™Y™\™[˜ÙQ\]X[Ê[X‹‘Ù]ÛÛ\Û™[Üš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“[X“]\ØÛOŠ
K[X‹“[X“]\ØÛJJJBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_H[XˆÛ[X‹šYH\È›İ›İ[™È]ÈXİÜ‹[İÛ™Y\Ú[ÛÙŞH[™]\ØÛHÛÛ\Û™[ÈŠNÂˆBˆYŠ\]Z\Y[O[[
^ÂˆÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK“ÜšYÚ[˜[XİÜ’Y[]K”™\]Z\™Q\]Z\Y[
[›ÙK\]Z\Y[
NÂˆYŠX™Z]š[İ\œË[JÛÛ\Û™[O”™Y™\™[˜ÙQ\]X[ÊÛÛ\Û™[\]Z\Y[
JJBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÚ\˜Xİ\›ÙH\]Z\Y[\È›İİÛ™YH]ÈİÛˆ™Y˜XˆÜ˜\ŠNÂˆBˆBˆ\Ú[ÛÙÚY\ËY
\Ú[ÛÙŞJNØÛÛ›Û\œËY
ÛÛ›Û\ŠNÜšYÚÛÛ™\œËY
šYÚÛÛ™\ŠNÛYÛÛ™\œËY
YÛÛ™\ŠNÙ\]Z\Y[™YœËY
\]Z\Y[
NÂˆ[X‘Ü˜\ËY
[›ÙK›[XœË•Ğ\œ˜^J
JNÂˆ˜\ˆÜšYÚ[˜[›ÙOXXİÜ‹’][L‹‘Ù]ÛÛ\Û™[Ò[Ú[™[[š]Q[™Ú[™K“[Û›Ğ™Z]š[İ\ŠYJK”Ú[™ÛJÛÛ\Û™[O˜ÛÛ\Û™[‘Ù]\J
K“˜[YOOHÚ\˜Xİ\›ÙHŠNÂˆ˜\ˆİ\™QšY[[ÜšYÚ[˜[›ÙK‘Ù]\J
K‘Ù]šY[
š]™XÛİ™\Pİ\™H‹›YÜÊOÏİ›İÈ™]ÈZ\ÜÚ[™ÑšY[^Ù\[ÛŠÜšYÚ[˜[›ÙK‘Ù]\J
K‘[˜[YKš]™XÛİ™\Pİ\™HŠNÂˆ˜\ˆ]]Ü™Yİ\™OXİ\™QšY[‘Ù]˜[YJÜšYÚ[˜[›ÙJH\È[š]Q[™Ú[™K[š[X][Ûİ\™OÏİ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÛİ\˜ÙH™Y˜Xˆ\È›È]]Ü™Y]™XÛİ™\Pİ\™HŠNÂˆ˜\ˆÛÛ™Yİ\™OXİ\™QšY[‘Ù]˜[YJXİÜ›ÙY\ÖÌJH\È[š]Q[™Ú[™K[š[X][Ûİ\™OÏİ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÛÛ™HÜİ]™XÛİ™\Pİ\™HŠNÂˆ˜\ˆ]]Ü™YÙ^\ÏX]]Ü™Yİ\™KšÙ^\Îİ˜\ˆÛÛ™YÙ^\ÏXÛÛ™Yİ\™KšÙ^\ÎÂˆYŠ™Y™\™[˜ÙQ\]X[Ê]]Ü™Yİ\™KÛÛ™Yİ\™J_]]Ü™Yİ\™Kœ™UÜ˜\[ÙHOXÛÛ™Yİ\™Kœ™UÜ˜\[Ù_]]Ü™Yİ\™KœÜİÜ˜\[ÙHOXÛÛ™Yİ\™KœÜİÜ˜\[Ù_]]Ü™Yİ\™K˜Û[\İ]ÚYHOXÛÛ™Yİ\™K˜Û[\İ]ÚY_]]Ü™Yİ\™KœÙ\šX[^™Y™R[™š[š]HOXÛÛ™Yİ\™KœÙ\šX[^™Y™R[™š[š]_]]Ü™Yİ\™KœÙ\šX[^™YÜİ[™š[š]HOXÛÛ™Yİ\™KœÙ\šX[^™YÜİ[™š[š]_]]Ü™Yİ\™KœÙ\šX[^™Y™\œÚ[ÛˆOXÛÛ™Yİ\™KœÙ\šX[^™Y™\œÚ[ÛŸ]]Ü™Yİ\™KœÙ\šX[^™Y›İ][Û“Ü™\ˆOXÛÛ™Yİ\™KœÙ\šX[^™Y›İ][Û“Ü™\Ÿ]]Ü™YÙ^\Ë“[™İOXÛÛ™YÙ^\Ë“[™İ
]›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÛÛ™HÚ[™ÙY]]Ü™Y]™XÛİ™\Pİ\™HY]Y]HŠNÂˆ›ÜŠ˜\ˆÙ^R[™^LÚÙ^R[™^]]Ü™YÙ^\Ë“[™İÚÙ^R[™^
ÊÊ^İ˜\ˆÛİ\˜ÙRÙ^OX]]Ü™YÙ^\ÖÚÙ^R[™^Nİ˜\ˆÛÛ™RÙ^OXÛÛ™YÙ^\ÖÚÙ^R[™^NÚYŠ\Ûİ\˜ÙRÙ^K[YK‘\]X[ÊÛÛ™RÙ^K[YJ_\Ûİ\˜ÙRÙ^K˜[YK‘\]X[ÊÛÛ™RÙ^K˜[YJ_\Ûİ\˜ÙRÙ^Kš[•[™Ù[‘\]X[ÊÛÛ™RÙ^Kš[•[™Ù[
_\Ûİ\˜ÙRÙ^K›İ][™Ù[‘\]X[ÊÛÛ™RÙ^K›İ][™Ù[
_Ûİ\˜ÙRÙ^K[™Ù[[ÙHOXÛÛ™RÙ^K[™Ù[[Ù_Ûİ\˜ÙRÙ^KœÙ\šX[^™Y™\œÚ[ÛˆOXÛÛ™RÙ^KœÙ\šX[^™Y™\œÚ[ÛŠ]›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	ØXİÜ‹’][L_HÛÛ™HÚ[™ÙY]]Ü™Y]™XÛİ™\Pİ\™HÙ^HÚÙ^R[™^HŠNßBˆİ\™\ËY
ÛÛ™Yİ\™JNÂˆÛÛœÛÛK•Üš]S[™JŞ\İ[K•^’œÛÛ‹’œÛÛ”Ù\šX[^™\‹”Ù\šX[^™J™]ŞÛ[ÙO\[“Y™XŞXÛOÈ›Y™XŞXÛHˆš[œİ[X][Ûˆ‹™Y˜XXXİÜ‹’][LK[œİ[X]Yœ›ÛSÜšYÚ[˜[™Y˜X]YKXİ]™P™Z]š[İ\œÏXXİ]™K“[™İ]ØZÙOX]ÛÚÙ[‹Û‘[˜X›OY[˜X›Yİ\\İ\YÚ\˜Xİ\›ÙOXXİÜ›ÙY\Ë“[™İÜšYÚ[˜[RĞÛÛ›Û\XÛÛ›Û\ˆO[[ÜšYÚ[˜[\Ú[ÛÙŞO\\Ú[ÛÙŞHO[[\İ\]Z\Y[š^\™P]XÚYY\]Z\Y[O[[]™XÛİ™\Pİ\™RÙ^\ÏXÛÛ™Yİ\™KšÙ^\Ë“[™İJJNÂˆBˆYŠ™Y™\™[˜ÙQ\]X[Ê›ÙY\ÖÌK›ÙY\ÖÌWJ_™Y™\™[˜ÙQ\]X[Ê›ÙY\ÖÌK™Ø[YSØš™Xİ›ÙY\ÖÌWK™Ø[YSØš™Xİ
_™Y™\™[˜ÙQ\]X[Êİ\™\ÖÌKİ\™\ÖÌWJJ]›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ“Ü\˜]Üˆ[™ØØİ\[Ú\™HHÚ\˜Xİ\›ÙHÜˆ]]Ü™Y[š[X][Ûİ\™H[œİ[˜ÙHŠNÂˆYŠ[“Y™XŞXÛI‰Š™Y™\™[˜ÙQ\]X[Ê\Ú[ÛÙÚY\ÖÌK\Ú[ÛÙÚY\ÖÌWJ_™Y™\™[˜ÙQ\]X[ÊÛÛ›Û\œÖÌKÛÛ›Û\œÖÌWJ_™Y™\™[˜ÙQ\]X[ÊšYÚÛÛ™\œÖÌKšYÚÛÛ™\œÖÌWJ_™Y™\™[˜ÙQ\]X[ÊYÛÛ™\œÖÌKYÛÛ™\œÖÌWJ_
\]Z\Y[™YœÖÌHO[[	‰”™Y™\™[˜ÙQ\]X[Ê\]Z\Y[™YœÖÌK\]Z\Y[™YœÖÌWJJJJBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ“Ü\˜]Üˆ[™ØØİ\[Ú\™H\Ú[ÛÙŞKRÈÛÛ›Û\‹\›HÛÛ™\œËÜˆ[š]X[^™Y\]Z\Y[ŠNÂˆYŠ[“Y™XŞXÛJ^Âˆ˜\ˆÜ\˜]Ü›ÙOJÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙKÚ\˜Xİ\›ÙJX›ÙY\ÖÌNÂˆ˜\ˆÜ\˜]ÜÛÛ›Û\XÛÛ›Û\œÖÌNÂˆYŠšYÚÛÛ™\œÖÌKš[\œÛ][ÛË›[™İOLŸYÛÛ™\œÖÌKš[\œÛ][ÛË›[™İOLŠBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	“ÜšYÚ[˜[Ü\˜]Üˆ\›H[\œÛ][Ûˆİ\™\ÈÙ\™H›İ\Ù\šX[^™Y
šYÚ^ÜšYÚÛÛ™\œÖÌKš[\œÛ][ÛË›[™İKY^ÛYÛÛ™\œÖÌKš[\œÛ][ÛË›[™İJHŠNÂˆÜ\˜]ÜÛÛ›Û\‹•[˜\›YY

NÂˆYŠ[Ü\˜]ÜÛÛ›Û\‹’\Ô^Z[™ßˆT™Y™\™[˜ÙQ\]X[ÊšYÚÛÛ™\œÖÌK•˜[œÚ][Û•\™Ù]œ\™[Ü\˜]Ü›ÙK”[š\Ë˜[œÙ›Ü›J_ˆT™Y™\™[˜ÙQ\]X[ÊYÛÛ™\œÖÌK•˜[œÚ][Û•\™Ù]œ\™[Ü\˜]Ü›ÙK”[š\Ë˜[œÙ›Ü›JJBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ“ÜšYÚ[˜[Ü\˜]ÜˆY›İİ\]È]]Ü™YÛËX\›H[š\È˜[œÚ][ÛˆÛˆHXİÜ‹[İÛ™YšYÈŠNÂˆ˜\ˆ[YTÙ]\]\[ÙŠ[š]Q[™Ú[™K•[YJK‘Ù]›Ü\J˜[Y[ÙŠ[š]Q[™Ú[™K•[YK[YJKŞ\İ[K”™Y›Xİ[Û‹š[™[™Ñ›YÜË”İ]XßŞ\İ[K”™Y›Xİ[Û‹š[™[™Ñ›YÜË”X›XßŞ\İ[K”™Y›Xİ[Û‹š[™[™Ñ›YÜË“›Û”X›XÊHK‘Ù]Ù]Y]Ù
YJHNÂˆ˜\ˆ[TÙ]\]\[ÙŠ[š]Q[™Ú[™K•[YJK‘Ù]›Ü\J˜[Y[ÙŠ[š]Q[™Ú[™K•[YK™[U[YJKŞ\İ[K”™Y›Xİ[Û‹š[™[™Ñ›YÜË”İ]XßŞ\İ[K”™Y›Xİ[Û‹š[™[™Ñ›YÜË”X›XÊHK‘Ù]Ù]Y]Ù
YJHNÂˆ˜\ˆÛÛ›Û\•\]O]\[ÙŠÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK’RĞ\›PÛÛ›Û\ŠK‘Ù]Y]Ù
•\]H‹›YÜÊHNÂˆ˜\ˆÛÛ™\•\]O]\[ÙŠÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK\›RRÔÛÛ™\ŠK‘Ù]Y]Ù
•\]H‹›YÜÊHNÂˆ˜\ˆÙ\]Y[˜ÙTİ\U[š]Q[™Ú[™K•[YK[YNÂˆ›ÜŠ˜\ˆXÚÏLNİXÚÏM	‰›Ü\˜]ÜÛÛ›Û\‹’\Ô^Z[™ÎİXÚÊÊÊ^ÂˆÛÛœİ›Ø]İ\KŒYÂˆ[YTÙ]\‹’[›ÚÙJ[™]ÈØš™Xİ×^ÜÙ\]Y[˜ÙTİ\
İXÚÊœİ\JNÂˆ[TÙ]\‹’[›ÚÙJ[™]ÈØš™Xİ×^Üİ\JNÂˆÛÛ›Û\•\]K’[›ÚÙJÜ\˜]ÜÛÛ›Û\‹[
NÂˆÛÛ™\•\]K’[›ÚÙJšYÚÛÛ™\œÖÌK[
NÂˆÛÛ™\•\]K’[›ÚÙJYÛÛ™\œÖÌK[
NÂˆBˆYŠÜ\˜]ÜÛÛ›Û\‹’\Ô^Z[™ß[Ü\˜]ÜÛÛ›Û\‹”šYÚÙ\]Y[˜Ù\‹’\Ô™XY_[Ü\˜]ÜÛÛ›Û\‹“YÙ\]Y[˜Ù\‹’\Ô™XY_ˆ\šYÚÛÛ™\œÖÌK“Û•\™Ù][YÛÛ™\œÖÌK“Û•\™Ù]ˆT™Y™\™[˜ÙQ\]X[ÊšYÚÛÛ™\œÖÌKšZÕ\™Ù]œ\™[Ü\˜]Ü›ÙK”[š\Ë˜[œÙ›Ü›J_ˆT™Y™\™[˜ÙQ\]X[ÊYÛÛ™\œÖÌKšZÕ\™Ù]œ\™[Ü\˜]Ü›ÙK”[š\Ë˜[œÙ›Ü›JJBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ	“ÜšYÚ[˜[Ü\˜]Üˆ]]Ü™Y[š\È˜[œÚ][ÛœÈY›İ™XXÚ›İ\™Ù]È[™ÛÛ\]H
^Z[™Ï^ÛÜ\˜]ÜÛÛ›Û\‹’\Ô^Z[™ßKšYÚ™XYO^ÛÜ\˜]ÜÛÛ›Û\‹”šYÚÙ\]Y[˜Ù\‹’\Ô™XY_KY™XYO^ÛÜ\˜]ÜÛÛ›Û\‹“YÙ\]Y[˜Ù\‹’\Ô™XY_KšYÚÛ•\™Ù]^ÜšYÚÛÛ™\œÖÌK“Û•\™Ù]KYÛ•\™Ù]^ÛYÛÛ™\œÖÌK“Û•\™Ù]KšYÚ\™[^ÜšYÚÛÛ™\œÖÌKšZÕ\™Ù]œ\™[Ë›˜[Y_KY\™[^ÛYÛÛ™\œÖÌKšZÕ\™Ù]œ\™[Ë›˜[Y_JHŠNÂˆÛÛœÛÛK•Üš]S[™JŞ\İ[K•^’œÛÛ‹’œÛÛ”Ù\šX[^™\‹”Ù\šX[^™J™]ŞÜ™Y˜XH“Ü\˜]Üˆ‹]]Ü™YRÔÙ\]Y[˜ÙOH•[˜\›YY‹˜[œÚ][Û•\™Ù]H”[š\È‹šYÚÛ•\™Ù]\šYÚÛÛ™\œÖÌK“Û•\™Ù]YÛ•\™Ù][YÛÛ™\œÖÌK“Û•\™Ù]›İÙ\]Y[˜Ù\œÔ™XYO]Y_JJNÂˆYŠ[X‘Ü˜\ÖÌK“[™İO[[X‘Ü˜\ÖÌWK“[™İ[X‘Ü˜\ÖÌK•Ú\™J
[X‹[™^
OO”™Y™\™[˜ÙQ\]X[Ê[X‹[X‘Ü˜\ÖÌWVÚ[™^JJK[J
JBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ“Ü\˜]Üˆ[™ØØİ\[Ú\™HÛ™HÜˆ[Ü™HÜšYÚ[˜[[XˆÛÛ\Û™[ÈŠNÂˆ˜\ˆÜ\˜]Ü›ÛÙ™Y›Ü™O\\Ú[ÛÙÚY\ÖÌK›ÛÙ›Û[YNİ˜\ˆØØİ\[›ÛÙ™Y›Ü™O\\Ú[ÛÙÚY\ÖÌWK›ÛÙ›Û[YNÜ\Ú[ÛÙÚY\ÖÌK”™[[İ™P›ÛÙ
LŠNÂˆYŠ\Ú[ÛÙÚY\ÖÌK›ÛÙ›Û[YO[Ü\˜]Ü›ÛÙ™Y›Ü™_\Ú[ÛÙÚY\ÖÌWK›ÛÙ›Û[YHO[ØØİ\[›ÛÙ™Y›Ü™JBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ“Ü\˜]Üˆ\Ú[ÛÙŞH]]][ÛˆÚ[™ÙYÜˆ˜Z[YÈ\ÛÛ]HØØİ\[›ÛÙİ]HŠNÂˆ\]Z\Y[™YœÖÌK™Ø]^™OLNÂˆYŠ\]Z\Y[™YœÖÌWK™Ø]^™HOL
]›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ“Ü\˜]Üˆ\]Z\Y[š^\™H]]][ÛˆÚ[™ÙYØØİ\[\]Z\Y[İ]HŠNÂˆBˆÛÛœÛÛK•Üš]S[™J[“Y™XŞXÛOÈ“Ü\˜]Üˆ[™ØØİ\[Y™XŞXÛKÛÛ\Û™[İÛ™\œÚ\[™Ü\˜]Üˆ]]Ü™YÛËX\›H˜[œÚ][ÛˆÛÛ\][Ûˆ\ÜÙY[ˆÛ™H[[YUÛÜ›ˆˆ“Ü\˜]Üˆ[™ØØİ\[™Y˜Xˆ[œİ[X][Ûˆ[™ÛÛ\Û™[İÛ™\œÚ\\ÜÙY[ˆÛ™H[[YUÛÜ›ˆŠNÂˆ›Ü™XXÚ
˜\ˆXİÜˆ[ˆXİÜœÊ^Õ[š]Q[™Ú[™K“Øš™Xİ‘\İ›ŞR[[YYX]JXİÜ‹’][LÊNÕ[š]Q[™Ú[™K“Øš™Xİ‘\İ›ŞR[[YYX]JXİÜ‹’][LŠNßBˆ™]\›ÂŸBšYŠ\™ÜËÛÛZ[œÊ‹K\™Y˜X‹[Û›HŠJ^ÂˆYŠ\™ÜË“[™İOLŠ]›İÈ™]È\™İ[Y[^Ù\[ÛŠ‹K\™Y˜X‹[Û›H™\]Z\™\ÈHÙ[™\˜]YØÙ[™K\XÚØYÙKšœÛÛˆ][™ØYÈH™\Ù\™YÜ\˜]Üˆ[™ØØİ\[™Y˜XœÈŠNÂˆ˜\ˆXÚØYÙOQš[K”™XY[^
\™ÜÖÌWJNÂˆ›Ü™XXÚ
˜\ˆ˜[YH[ˆ™]Ö×^È“Ü\˜]Üˆ‹“ØØİ\[ŸJ^Âˆ˜\ˆ]I\ÜÙ]ËÔ™\Ûİ\˜Ù\ËØÚ\˜Xİ\œËŞÛ˜[Y_Kœ™Y˜XˆÂˆ˜\ˆÛÜ›[™]È[[YUÛÜ›

NÂˆ˜\ˆØYYTØÙ[™SØY\‹“ØY™Y˜XŠXÚØYÙK]ÛÜ›™\]Z\™PÛÛ\]NYJNÂˆÛÛœÛÛK•Üš]S[™JœÛÛ”Ù\šX[^™\‹”Ù\šX[^™JØYY”™\Ü

JJNÂˆ[š]Q[™Ú[™K“Øš™Xİ‘\İ›ŞR[[YYX]JØYYœ™Y˜X”›Ûİ
NÂˆBˆ™]\›ÂŸBÛÛœÛÛK•Üš]S[™J[[YPÛÛ˜XİË”[Š
JNÂÛÛœÛÛK•Üš]S[™JØÙ[™T™Y˜XÛÛ˜XİË”[Š
JNÂÛÛœÛÛK•Üš]S[™J\›TÙ\]Y[˜ÙPÛÛ˜XİË”[Š™Ø[YKØ\ÜÙ]ËÛÜšYÚ[˜[X\›K\Ù\]Y[˜Ù\ËšœÛÛˆŠJNÂšYŠš[K‘^\İÊ™Ø[YKØ\ÜÙ]ËÜ™\Ù[][Û‹šœÛÛ‹™ŞˆŠJPÛÛœÛÛK•Üš]S[™J™\Ù[][ÛÛÛ˜XİË”[Š™Ø[YKØ\ÜÙ]ËÜ™\Ù[][Û‹šœÛÛ‹™ŞˆŠJNÂšYŠš[K‘^\İÊ™Ø[YKØ\ÜÙ]ËİÙX\Û‹\[[YKšœÛÛ‹™ŞˆŠJPÛÛœÛÛK•Üš]S[™JÙX\ÛÛÛ˜XİË”[Š™Ø[YKØ\ÜÙ]ËİÙX\Û‹\[[YKšœÛÛ‹™ŞˆŠJNÂšYŠš[K‘^\İÊ™Ø[YKØ\ÜÙ]ËØ˜[\İXÜËšœÛÛ‹™ŞˆŠJPÛÛœÛÛK•Üš]S[™J˜[\İXĞÛÛ˜XİË”[Š
JNÂšYŠ\™ÜË“[™İŒ
^Âˆ˜\ˆXÚØYÙOQš[K”™XY[^
\™ÜÖÌJNÂˆ›Ü™XXÚ
˜\ˆ][ˆ™]Ö×^È\ÜÙ]ËÔØÙ[™\ËÓX\šÜÛX[ˆ˜[™ÙK[š]H‹\ÜÙ]ËÔØÙ[™\ËÓZ\ÜÚ[ÛˆK[š]HŸJ^Âˆ˜\ˆØYYTØÙ[™SØY\‹“ØY
XÚØYÙK]
NÂˆYŠØYY[œ™\ÛÛ™Y™Y™\™[˜Ù\ËÛİ[Œ
]›İÈ™]È^Ù\[ÛŠ•[œ™\ÛÛ™Yİ\ÜYšY[ÈŠNÂˆYŠ]OH\ÜÙ]ËÔØÙ[™\ËÓX\šÜÛX[ˆ˜[™ÙK[š]HŠ^Âˆ˜\ˆÛ\ÜÏ[ØYY›Øš™XİË•˜[Y\Ë“Ù•\OÜš\Ú\Ë”[[YK‘[Ûİ\˜ÙK˜[\İXÕÚ[™İÏŠ
Bˆ”Ù[Xİ
Ú[™İÏOÚ[™İË^\™JK‘š\œİÜ‘Y˜][
^\™OO^\™OË“ÜšYÚ[˜[\ÜÙ]]OH\ÜÙ]ËÕ^\™L‘İÚ[™İÈÛ\ÜËœ™ÈŠNÂˆYŠÛ\ÜÏO[[Û\ÜË“ÜšYÚ[˜[\ÜÙ]İZYOH˜˜ŒÙÌØ™M˜Y˜ÍØLMŒØM˜ÌÍÌÎMØXHŸÛ\ÜË“ÜšYÚ[˜[\ÜÙ]š[RQOHŒŸˆÛ\ÜË“ÜšYÚ[˜[[XYÙP]\ÏO[[ÛÛ™\•Ò^İš[™ÊŞ\İ[K”ÙXİ\š]KÜ\ÙÜ˜\K”ÒLM‹’\Ú]JÛ\ÜË“ÜšYÚ[˜[[XYÙP]\ÊJK•ÓİÙ\’[˜\šX[

HOH˜˜NLÙ™NXXÌYX™˜ÌÌÍNÌÙÙŒÍ™ÌYL˜MØNYHŸˆYÛ\ÜË’[\Ü\”Ûİ\˜ÙU^ÛÛZ[œÊˆ^\™Q›Ü›X]ˆWˆ‹İš[™ĞÛÛ\\š\ÛÛ‹“Ü™[˜[
JBˆ›İÈ™]È[˜[YÜ\˜][Û‘^Ù\[ÛŠ“ÜšYÚ[˜[X\šÜÛX[ˆÚ[™İÈ^\™HÜİ]ÈÕRQš[RQ‘ÈÛİ\˜ÙKÜˆT‘ĞŒÌˆ[\Ü\ˆY]Y]HŠNÂˆBˆÛÛœÛÛK•Üš]S[™JœÛÛ”Ù\šX[^™\‹”Ù\šX[^™JØYY”™\Ü

JJNÂˆÛÛœÛÛK•Üš]S[™JœÛÛ”Ù\šX[^™\‹”Ù\šX[^™JØÙ[™SØY\‹•™\šYR[\ÜY˜^JØYY
JJNÂˆBˆ›Ü™XXÚ
˜\ˆ][ˆ™]Ö×^È\ÜÙ]ËÔ™\Ûİ\˜Ù\ËİÙX\ÛœËØ[[][š][Û‹ÍKM‹R’œ™Y˜Xˆ‹\ÜÙ]ËÔ™\Ûİ\˜Ù\ËİÙX\ÛœËØ[[][š][Û‹ÌÌUÚ[‹P”Ôœ™Y˜XˆŸJBˆÛÛœÛÛK•Üš]S[™JœÛÛ”Ù\šX[^™\‹”Ù\šX[^™J[[][š][Û“ØY\‹•™\šYSÜšYÚ[˜[™Y˜XŠXÚØYÙK]
JJNÂˆÛÛœÛÛK•Üš]S[™JœÛÛ”Ù\šX[^™\‹”Ù\šX[^™J[[][š][Û“ØY\‹•™\šYSÜšYÚ[˜[™\Ûİ\˜ÙJXÚØYÙKKM‹R’ŠJJNÂˆÛÛœÛÛK•Üš]S[™J™\Ûİ\˜ÙT™Y˜XÛÛ˜XİË”[ŠXÚØYÙJJNÂˆÛÛœÛÛK•Üš]S[™JÜ]Û™\‘Ø[Y\^PÛÛ˜XİË”[ŠXÚØYÙJJNÂŸB‚œÙX[YÛ\ÜÈZ\ÜÚ[ÛÛÛ˜XİÚ[[]Y[Ñš]™\’P]Y[ĞÛ\^X˜XÚÑš]™\‹QY™\œ™Y]Y[ÔÛİ\˜ÙQš]™\ˆÂˆX›XÈ›ÛÛ\Ô^Z[™ÏO™˜[ÙNÂˆX›XÈ›ÚY^SÛ™TÚİ
]Y[ĞÛ\Û\›Ø]›Û[YK[š[Üš]J^ßBˆX›XÈ›ÚY^J]Y[ĞÛ\Û\›Ø]›Û[YK›Ø]]Ú›ÛÛÛÜ›ÛÛ]]Y[š[Üš]J^ßBˆX›XÈ›ÚYİÜ

^ßBŸBœÙX[YÛ\ÜÈZ\ÜÚ[ÛÛÛ˜XİÚ[[Z^\‘š]™\•[š]Q[™Ú[™K]Y[Ë’P]Y[ÓZ^\‘š]™\ˆÂˆX›XÈ›ÛÛÙ]›Ø]
İš[™È^ÜÙY\˜[Y]\‹›Ø]˜[YJOO™˜[ÙNÂˆX›XÈ›ÚY˜[œÚ][Û•Ê[š]Q[™Ú[™K]Y[Ë]Y[ÓZ^\”Û˜\ÚİÛ˜\Úİ›Ø][YUÔ™XXÚ
^ßBŸBœÙX[YÛ\ÜÈZ\ÜÚ[ÛÛÛ˜Xİ]X[]TÙ][™ÜÑš]™\•[š]Q[™Ú[™K’T]X[]TÙ][™ÜÑš]™\ˆÂˆX›XÈÙ]Šİš[™È›Ü\JOO™Y˜][ÂˆX›XÈ›ÚYÙ]Šİš[™È›Ü\K˜[YJ^ßBŸBœÙX[YÛ\ÜÈZ\ÜÚ[ÛÛÛ˜XİØÙ[™SX[˜YÙ\‘š]™\•[š]Q[™Ú[™K”ØÙ[™SX[˜YÙ[Y[’TØÙ[™SX[˜YÙ\‘š]™\ˆÂˆX›XÈ[š]Q[™Ú[™K”ØÙ[™SX[˜YÙ[Y[”ØÙ[™HÙ]Xİ]™TØÙ[™J
OO›™]ÊË“Z\ÜÚ[ÛˆHŠNÂˆX›XÈ›ÚYØYØÙ[™J[Z[[™^
OO›İÈ™]È›İİ\ÜY^Ù\[ÛŠ“Z\ÜÚ[ÛˆÛÛ˜XİÙ\È›İÚ[™ÙHØÙ[™\ÈŠNÂˆX›XÈ›ÚYØYØÙ[™Jİš[™ÈØÙ[™S˜[YJOO›İÈ™]È›İİ\ÜY^Ù\[ÛŠ“Z\ÜÚ[ÛˆÛÛ˜XİÙ\È›İÚ[™ÙHØÙ[™\ÈŠNÂŸB
using UnityEngine;

static class TextureTransferContracts {
    public static int Run() {
        int count=0;
        void Check(bool value,string message) {if(!value)throw new Exception(message);count++;}
        void Throws<T>(Action action) where T:Exception {try{action();}catch(T){count++;return;}throw new Exception("Expected "+typeof(T).Name);}
        var world=new RuntimeWorld();world.MakeCurrent();
        var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
        source.SetPixels([new(1,0,0,1),new(0,1,0,1),new(0,0,1,1),new(1,1,1,1)]);
        var target=new RenderTexture(2,2,24,RenderTextureFormat.ARGB32);
        Throws<NotSupportedException>(()=>Graphics.Blit(source,target));
        Check(!target.IsCreated(),"unbound transfers do not claim target allocation");
        var backend=new CpuTextureTransferDriver(world);backend.Bind();
        Graphics.Blit(source,target);
        Check(target.IsCreated()&&target.depth==24,"original target dimensions and depth metadata survive real allocation");
        RenderTexture.active=target;
        var readback=new Texture2D(4,3,TextureFormat.RGBA32,false);
        readback.ReadPixels(new Rect(0,1,2,1),1,1);
        Check(readback.GetPixel(1,1).b==1&&readback.GetPixel(1,1).r==0&&readback.GetPixel(2,1).r==1,"readback keeps bottom-left source orientation and destination offsets");
        Check(readback.GetPixel(0,0).a==0&&readback.GetPixel(3,2).a==0,"subrectangle readback preserves untouched pixels");
        source.SetPixels([Color.black,Color.black,Color.black,Color.black]);
        var copy=new Texture2D(2,2,TextureFormat.RGBA32,false);copy.ReadPixels(new Rect(0,0,2,2),0,0);
        Check(copy.GetPixel(0,0).r==1&&copy.GetPixel(0,1).b==1,"blit is a pixel snapshot independent of later source edits");
        var secondTarget=new RenderTexture(2,2,24,RenderTextureFormat.ARGB32);Graphics.Blit(target,secondTarget);
        RenderTexture.active=secondTarget;copy.ReadPixels(new Rect(0,0,2,2),0,0);
        Check(copy.GetPixel(1,0).g==1&&copy.GetPixel(1,1).r==1,"render-to-render copy retains stored mask pixels");
        Throws<ArgumentOutOfRangeException>(()=>copy.ReadPixels(new Rect(1,0,2,2),0,0));
        Check(copy.GetPixel(0,0).r==1,"failed readback leaves CPU pixels unchanged");
        Throws<ArgumentException>(()=>copy.SetPixels(0,0,2,2,[Color.clear]));
        Check(copy.GetPixel(1,0).g==1,"invalid bulk update has no partial writes");
        Throws<ArgumentOutOfRangeException>(()=>copy.SetPixels(0,0,2,2,[Color.clear,new(float.NaN,0,0,1),Color.clear,Color.clear]));
        Check(copy.GetPixel(0,0).r==1,"nonfinite color rejection occurs before committing a pixel patch");
        Check(copy.GetPixel(-1,0).g==1&&copy.GetPixel(2,0).r==1,"repeat addressing handles both negative and positive out-of-range coordinates");
        copy.wrapMode=TextureWrapMode.Clamp;
        Check(copy.GetPixel(-1,0).r==1&&copy.GetPixel(2,0).g==1,"clamp addressing preserves edge pixels for source tracing");
        var cloned=UnityEngine.Object.Instantiate(copy);
        Check(cloned.wrapMode==TextureWrapMode.Clamp,"texture cloning preserves wrap mode");
        target.Release();target.Release();
        Check(!target.IsCreated()&&secondTarget.IsCreated(),"release is idempotent and does not free another target");
        target.Create();RenderTexture.active=target;copy.ReadPixels(new Rect(0,0,2,2),0,0);
        Check(copy.GetPixel(0,0).a==0,"recreated CPU storage does not expose released mask pixels");
        var other=new RuntimeWorld();other.MakeCurrent();new CpuTextureTransferDriver(other).Bind();
        var otherTarget=new RenderTexture(2,2,24,RenderTextureFormat.ARGB32);
        Throws<InvalidOperationException>(()=>Graphics.Blit(source,otherTarget));
        Check(!otherTarget.IsCreated(),"cross-world source rejection occurs before destination allocation");
        Throws<InvalidOperationException>(()=>RenderTexture.active=secondTarget);
        world.MakeCurrent();
        Check(ReferenceEquals(RenderTexture.active,target),"world switch retains each backend's active target identity");
        RenderTexture.active=null;target.Release();secondTarget.Release();
        Throws<InvalidOperationException>(()=>copy.ReadPixels(new Rect(0,0,1,1),0,0));
        Throws<NotSupportedException>(()=>new Texture2D(2,2,TextureFormat.RGBA32,true));
        var lightingGo=new GameObject("room lighting contracts",world);
        var sourceLight=lightingGo.AddComponent<Light>();var lightDriver=new LightProbe();sourceLight.Driver=lightDriver;
        lightDriver.Changes.Clear();sourceLight.enabled=false;sourceLight.shadows=LightShadows.Soft;
        Check(lightDriver.Changes.SequenceEqual(new[]{"enabled:False","shadows:Soft"}),"source light toggles and shadow quality reach the bound engine driver in order");
        Check(!sourceLight.enabled&&sourceLight.shadows==LightShadows.Soft,"source light state remains readable on the CPU");
        Throws<ArgumentOutOfRangeException>(()=>sourceLight.shadows=(LightShadows)99);
        var roomRenderer=lightingGo.AddComponent<MeshRenderer>();var firstMaterial=new Material();var secondMaterial=new Material();
        Material[] observedMaterials=null;roomRenderer.OnMaterialsAssigned=materials=>observedMaterials=materials;
        roomRenderer.materials=new[]{firstMaterial,secondMaterial};var materialSnapshot=roomRenderer.materials;
        Check(ReferenceEquals(roomRenderer.material,firstMaterial)&&materialSnapshot.Length==2&&ReferenceEquals(materialSnapshot[1],secondMaterial),"original renderer material array preserves assigned material identities");
        materialSnapshot[0]=new Material();
        Check(ReferenceEquals(roomRenderer.materials[0],firstMaterial),"renderer material arrays are independent snapshots");
        var emission=new Color(.2f,.4f,.6f,1);string colorProperty=null;Color observedColor=default;
        firstMaterial.OnColorAssigned=(property,color)=>{colorProperty=property;observedColor=color;};
        firstMaterial.SetColor("_EmissionColor",emission);
        Check(colorProperty=="_EmissionColor"&&firstMaterial.GetColor(colorProperty).g==.4f&&observedColor.b==.6f,"source emissive material values persist and reach the presentation observer");
        Check(observedMaterials.Length==2,"renderer assignment observer receives the complete material set");
        return count;
    }

    sealed class LightProbe:ILightDriver {
        public readonly List<string> Changes=new();
        public void SetEnabled(Light light,bool enabled)=>Changes.Add("enabled:"+enabled);
        public void SetShadows(Light light,LightShadows shadows)=>Changes.Add("shadows:"+shadows);
    }
}

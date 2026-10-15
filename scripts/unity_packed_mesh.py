"""Decode Unity's serialized packed mesh streams (LSB first).

Format reference: AssetStudio/Classes/AnimationClip.cs PackedFloatVector and
PackedIntVector, https://github.com/Perfare/AssetStudio . No axis conversion.
"""
def unpack(vector, floating=False):
    count=int(vector['m_NumItems']); bits=int(vector['m_BitSize'])
    if not count:return []
    if bits<=0 or bits>32:raise ValueError('Unsupported packed bit width')
    raw=bytes.fromhex(vector['m_Data']); mask=(1<<bits)-1
    if len(raw)*8<count*bits:raise ValueError('Truncated packed vector')
    values=[]
    for i in range(count):
        offset=i*bits; byte=offset//8; shift=offset%8
        value=(int.from_bytes(raw[byte:byte+(shift+bits+7)//8], 'little')>>shift)&mask
        values.append(value*float(vector['m_Range'])/mask+float(vector['m_Start']) if floating else value)
    return values

def compressed_mesh(fields):
    packed=fields['m_CompressedMesh']; points=unpack(packed['m_Vertices'],True)
    indices=unpack(packed['m_Triangles'])
    if not points or len(points)%3 or len(indices)%3:raise ValueError('Invalid packed triangle mesh')
    if any(s.get('topology',0)!=0 for s in fields['m_SubMeshes']):raise ValueError('Non-triangle mesh topology')
    vertices=[points[i:i+3] for i in range(0,len(points),3)]
    triangles=[indices[i:i+3] for i in range(0,len(indices),3)]
    if any(i<0 or i>=len(vertices) for i in indices):raise ValueError('Packed index outside vertex buffer')
    return vertices,triangles


def compressed_normals(fields):
    """Decode original Unity packed normal XY+signs (AssetStudio Mesh.cs).

    Unity's m_Normals serializes XY pairs, with Z reconstructed from unit
    length and sign bits in m_NormalSigns. This returns ORIGINAL Unity-space
    vectors; callers must apply the established Unity-to-browser Z reflection.
    No surface-normal estimation is performed.
    """
    import math
    packed=fields['m_CompressedMesh']
    normals=packed.get('m_Normals',{})
    signs=packed.get('m_NormalSigns',{})
    n=int(normals.get('m_NumItems',0))
    if not n:return []
    if n%2:raise ValueError('Original packed normals require XY pairs')
    vals=unpack(normals,True)
    sign_values=unpack(signs)
    if len(sign_values)!=n//2:
        raise ValueError('Original packed normal signs do not match vertices')
    result=[]
    for i in range(n//2):
        x,y=vals[2*i:2*i+2]
        if not math.isfinite(x) or not math.isfinite(y):
            raise ValueError('Non-finite original packed normal')
        zsqr=1-x*x-y*y
        if zsqr>=0:
            z=math.sqrt(zsqr)
        else:
            # Exact Unity/AssetStudio negative Z-square fallback normalizes XY
            # and reconstructs Z=0, rather than inventing a face normal.
            length=math.hypot(x,y)
            if length<=0:raise ValueError('Degenerate original packed XY normal')
            x,y,z=x/length,y/length,0.0
        if sign_values[i] not in (0,1):
            raise ValueError('Original packed normal sign must be a bit')
        if sign_values[i]==0:z=-z
        result.append([x,y,z])
    return result

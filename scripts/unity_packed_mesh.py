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

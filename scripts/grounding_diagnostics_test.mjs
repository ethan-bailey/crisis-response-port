import test from 'node:test';
import assert from 'node:assert/strict';
import {describeGrounding} from '../game/grounding-diagnostics.mjs';

test('grounding diagnostic reports each root and respects authored capsule axis',()=>{
  const d=describeGrounding({sourcePosition:[1,1.5,2],physics:{x:1,y:1.2,z:2},projectedPosition:[1,1.2,2],
    capsule:{m_Radius:.3,m_Height:1,m_Direction:0,m_Center:{x:0,y:.235,z:0}},
    center:{x:0,y:-.64,z:0},boxes:[{name:'floor',position:[0,-.1,0],halfExtents:[2,.1,1]}]});
  assert.deepEqual(d.sourceRoot,[1,1.5,2]);
  assert.deepEqual(d.contactRoot,[1,1.2,2]);
  assert.deepEqual(d.projectedRoot,[1,1.2,2]);
  assert.equal(d.capsule.direction,0);
  assert.equal(d.capsule.verticalHalfExtent,.3);
  assert.equal(d.capsule.bottom,.56);
  assert.deepEqual(d.floorCandidates,[{name:'floor',top:0}]);
});

test('vertical capsule uses half authored height, not radius',()=>{
  const d=describeGrounding({sourcePosition:[0,2,0],physics:null,projectedPosition:[0,2,0],
    capsule:{m_Radius:.3,m_Height:1,m_Direction:1},center:{y:0}});
  assert.equal(d.capsule.verticalHalfExtent,.5);
  assert.equal(d.capsule.bottom,1.5);
});

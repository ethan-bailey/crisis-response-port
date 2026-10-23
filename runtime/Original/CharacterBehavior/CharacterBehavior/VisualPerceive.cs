using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Perception")]
	[TaskIcon("Assets/Editor/Behavior icons/eye.png")]
	public class VisualPerceive : Action
	{
		public SharedTarget target;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("For how long should a target decay before it is discarded? In seconds.")]
		public float targetDecayTime = 1f;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			self.UpdateVisibleBodies();
			SelectTarget();
			DecayTarget();
			DetectGrenades();
			return TaskStatus.Running;
		}

		private void SelectTarget()
		{
			if (self.Blackboard.Enemies.Count == 0)
			{
				return;
			}
			Target target = null;
			float num = float.NegativeInfinity;
			foreach (Target enemy in self.Blackboard.Enemies)
			{
				float num2 = TargetScore(enemy);
				if (!(num2 <= 0f) && !(num2 <= num))
				{
					target = enemy;
					num = num2;
				}
			}
			if (target != this.target.Value)
			{
				self.Blackboard.targetFirstSeenTimestamp = Time.time;
			}
			this.target.Value = target;
			self.Blackboard.targetLastSeenTimestamp = Time.time;
		}

		private void DecayTarget()
		{
			self.Blackboard.TargetDecayTime = Time.time - self.Blackboard.targetLastSeenTimestamp;
			self.Blackboard.TargetVisibleTime = Time.time - self.Blackboard.targetFirstSeenTimestamp;
			if (!(self.Blackboard.TargetDecayTime < targetDecayTime))
			{
				ClearTarget();
			}
		}

		private void ClearTarget()
		{
			target.Value = null;
			self.Blackboard.targetFirstSeenTimestamp = float.NegativeInfinity;
			self.Blackboard.targetLastSeenTimestamp = float.NegativeInfinity;
		}

		private float TargetScore(Target candidate)
		{
			float num = 0f;
			float num2 = Mathf.Clamp(10f - Mathf.Abs(candidate.transform.position.x - self.transform.position.x), 0f, 10f);
			float num3 = 0.25f;
			float num4 = 1f;
			float num5 = 1f;
			if (candidate.Body != null)
			{
				CharacterBody body = candidate.Body;
				num3 = ((!body.IsRagdoll) ? 1f : UnconsciousBodyInterestFactor(body.Physiology.UnconsciousTimestamp));
				num4 = ((!body.IsCompliant) ? 1f : 0.1f);
				num5 = ((!BodyHelpers.TargetFacesSubject(self, body)) ? 0.25f : 1f);
			}
			return num2 * num5 * num3 * num4;
		}

		private float UnconsciousBodyInterestFactor(float unconsciousTimestamp)
		{
			float num = 0f;
			switch (self.tempo)
			{
			case CharacterBody.Tempo.Quick:
				return 0f;
			case CharacterBody.Tempo.Slow:
				num = 1f;
				break;
			default:
				num = 0.5f;
				break;
			}
			return 1f - Mathf.Clamp((Time.time - unconsciousTimestamp) / num, 0f, 1f);
		}

		private void DetectGrenades()
		{
			Transform liveGrenadesContainer = self.Mission.LiveGrenadesContainer;
			if (liveGrenadesContainer.childCount == 0)
			{
				self.Blackboard.GrenadeSeenTimestamp = float.PositiveInfinity;
				self.Blackboard.PerceivedGrenade = null;
			}
			if (self.Blackboard.PerceivedGrenade != null)
			{
				return;
			}
			Grenade grenade = null;
			for (int i = 0; i < liveGrenadesContainer.childCount; i++)
			{
				Transform child = liveGrenadesContainer.GetChild(i);
				float f = child.transform.position.x - transform.position.x;
				if (!(Mathf.Abs(f) > self.VisibleRange) && Mathf.Sign(f) == self.Direction && !Physics.Raycast(transform.position + Vector3.up, self.transform.right, Mathf.Abs(f), Crisis.Runtime.FullSource.Masks.Instance.BallisticTargetsSansLimbs))
				{
					if (float.IsInfinity(self.Blackboard.GrenadeSeenTimestamp))
					{
						self.Blackboard.GrenadeSeenTimestamp = Time.time;
					}
					grenade = child.GetComponent<Grenade>();
					break;
				}
			}
			if (!(grenade == null) && !(Time.time - self.Blackboard.GrenadeSeenTimestamp < 0.4f))
			{
				self.Blackboard.PerceivedGrenade = grenade;
			}
		}
	}
}

using System.Collections;
using UnityEngine;

namespace Other
{
	[RequireComponent(typeof(ParticleSystem))]
	public class CFX_AutoDestructShuriken : MonoBehaviour
	{
		public bool OnlyDeactivate;

		private void OnEnable()
		{
			StartCoroutine(nameof(CheckIfAlive));
		}
	
		IEnumerator CheckIfAlive ()
		{
			if (OnlyDeactivate)
			{
				yield return new WaitForSeconds(0.5f);
				this.gameObject.SetActive(false);
			}
			else
			{
				while(true)
				{
					yield return new WaitForSeconds(0.5f);
					if (GetComponent<ParticleSystem>().IsAlive(true)) continue;
					if(OnlyDeactivate)
					{
#if UNITY_3_5
						this.gameObject.SetActiveRecursively(false);
#else
						this.gameObject.SetActive(false);
#endif
					}
					else
						GameObject.Destroy(this.gameObject);
					break;
				}
			}

		}
	}
}

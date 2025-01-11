// used to destroy structures on death
using Mirror;

namespace uSurvival
{
	public class OnDeathDestroy : NetworkBehaviour
	{
		[Server]
		public void OnDeath()
		{
			Destroy(gameObject);
		}
	}
}
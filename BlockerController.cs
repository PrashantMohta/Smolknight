using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmolKnight
{
    internal class BlockerController : MonoBehaviour
    {
        bool isClosed = false;
        void OnTriggerEnter2D(Collider2D col)
        {
            if (isClosed) { return; }
            if (col.gameObject.layer == (int)PhysLayers.PLAYER)
            {
                gameObject.Find("blocker").SetActive(true);
                isClosed=true;
            }
        }
    }
}
    
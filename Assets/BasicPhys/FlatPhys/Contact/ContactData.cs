using UnityEngine;
using FixMath.NET;
namespace FlatPhysics.Contact
{
    public class ContactData
    {
        public FVector2 contactPos;
        public GameObject other;

        public ContactData(FVector2 cp, GameObject go)
        {
            contactPos = cp;
            other = go;
        }
    }
}
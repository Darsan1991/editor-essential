using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DGames.Essentials.Base
{
    public class MonoBehaviour : UnityEngine.MonoBehaviour, ISerializationCallbackReceiver
    {
       
       [HideInInspector]
       [SerializeField] protected List<FieldVsObject> fieldVsObjects = new();


        public void OnBeforeSerialize()
        {
            fieldVsObjects.Clear();
            var fields = SerializeInterfaceField.GetAllSerializedInterfaceFields(GetType()).ToList();
     

            foreach (var field in fields.Where(field =>
                         field.GetValue(this)?.GetType().IsSubclassOf(typeof(UnityEngine.Object)) ?? false))
            {
                fieldVsObjects.Add(new FieldVsObject
                {
                    name = field.Name,
                    obj = (UnityEngine.Object)field.GetValue(this)
                });
            }
        }

        public void OnAfterDeserialize()
        {
            if (fieldVsObjects.Count == 0)
            {
                return;
            }

            var fields = SerializeInterfaceField.GetAllSerializedInterfaceFields(GetType()).ToList();

            foreach (var fieldVsObject in fieldVsObjects)
            {
                var field = fields.FirstOrDefault(f => f.Name == fieldVsObject.name);
                if (field != null)
                {
                    field.SetValue(this, fieldVsObject.obj);
                }
            }
        }


        [Serializable]
        public struct FieldVsObject
        {
            public string name;
            public UnityEngine.Object obj;
        }
    }

  
}
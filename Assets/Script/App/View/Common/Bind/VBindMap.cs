﻿using UnityEngine;
using UnityEngine.UI;

namespace App.View.Common.Bind
{

    public class VBindMap : VBindBase
    {
        public override void UpdateView()
        {
            object val = this.GetByPath(BindPath);
            Debug.LogError("val="+ val);
            if (val == null)
            {
                return;
            }
            GameObject obj = Object.Instantiate(val as GameObject);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

}
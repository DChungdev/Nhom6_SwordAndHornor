using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThanhMau : MonoBehaviour
{
    public Image thanhMau;
    
    public void CapNhatThanhMau(float mauhientai, float mautoida)
    {
        thanhMau.fillAmount = mauhientai / mautoida;
    }
}

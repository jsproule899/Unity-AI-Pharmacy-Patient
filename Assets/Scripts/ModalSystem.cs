using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalSystem : MonoBehaviour
{

    public UIController UI;
    public ModalSystem modal;


    
public void Show()
{
    modal.gameObject.SetActive(true);
    UI.recordButton.interactable = false;

}

public void Hide()
{
    modal.gameObject.SetActive(false);
    UI.recordButton.interactable = true;
}

public InputField[] GetInputs()
{
    return modal.GetComponentsInChildren<InputField>();
}

public TMP_Dropdown[] GetDropdowns()
{
    return modal.GetComponentsInChildren<TMP_Dropdown>();
}

public void DeactivateInput(string name)
{
    modal.RecursiveFindChild(this.transform, name).gameObject.SetActive(false);

}

public void ActivateInput(string name)
{
    modal.RecursiveFindChild(this.transform, name).gameObject.SetActive(true);
}

public void ClearInputs()
{
    modal.GetComponentsInChildren<InputField>().ToList().ForEach(input => input.text = "");

}

public Transform RecursiveFindChild(Transform parent, string childName)
{
    foreach (Transform child in parent)
    {
        if (child.name == childName)
        {
            return child;
        }
        else
        {
            Transform found = RecursiveFindChild(child, childName);
            if (found != null)
            {
                return found;
            }
        }
    }
    return null;
}


}

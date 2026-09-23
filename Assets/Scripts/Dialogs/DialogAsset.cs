using System;
using System.Collections.Generic;
using DefenderOfDreams.Core;
using UnityEngine;

namespace DefenderOfDreams.Dialogs
{
    [Serializable]
    public class DialogLine
    {
        public string speaker;
        [TextArea(1, 3)]
        public string text;
        public string requiredFlag;
        public string setFlag;
        public int requiredMemoryId = -1;
    }

    [CreateAssetMenu(menuName = "DefenderOfDreams/Dialog", fileName = "NewDialog")]
    public class DialogAsset : ScriptableObject
    {
        public string dialogId = "dialog";
        public List<DialogLine> lines = new();
        public bool once = true;
        public string completionFlag;
    }
}

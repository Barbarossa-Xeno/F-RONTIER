using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace FRONTIER.Menu
{
    public class NumberAutoComma : MonoBehaviour
    {
        private TextMeshProUGUI textMeshPro = null;
        private string tempText = "";
        private string nowText
        {
            get { return textMeshPro != null ? textMeshPro.text : GetComponent<TextMeshProUGUI>().text; }
            set { textMeshPro.text = value; }
        }
        private bool isInsert
        {
            get
            {
                if (!nowText.Equals(tempText)) { return true; }
                else { return false; }
            }
        }
        private const int COMMA_INSERT_INTERVAL = 3;
        private char[] target;
        private int commmaCount
        {
            get
            {
                if ((float)target.Length / (float)COMMA_INSERT_INTERVAL > target.Length / COMMA_INSERT_INTERVAL)
                {
                    return target.Length + target.Length / COMMA_INSERT_INTERVAL;
                }
                else { return target.Length + target.Length / COMMA_INSERT_INTERVAL - 1; }
            }
        }

        void Start() => Initialize();

        void Update() => CheckUpdate();

        private void Initialize()
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
            textMeshPro.text = InsertComma();
            tempText = textMeshPro.text;
        }
        private void CheckUpdate()
        {
            if (isInsert)
            {
                textMeshPro = GetComponent<TextMeshProUGUI>();
                textMeshPro.text = InsertComma();
                tempText = textMeshPro.text;
            }
            if (nowText == null || nowText == "")
            {
                if (this.gameObject.name == "val.score")
                {
                    nowText = GameManager.instance.scoreData.Score.ToString();
                }
            }
        }
        private char[] CountCharacter()
        {
            string str = nowText;
            char[] characters = str.ToCharArray();
            return characters;
        }

        private string InsertComma()
        {
            target = CountCharacter();

            char[] newStr = null;
            try { newStr = new char[commmaCount]; }
            catch (System.OverflowException) { }
            if (target.Length > COMMA_INSERT_INTERVAL)
            {
                for (int i = 0, j = 0, k = 0; i < newStr.Length; i++)
                {
                    if (i > 0 && k > 0 && k % 3 == 0)
                    {
                        newStr[newStr.Length - 1 - i] = ',';
                        k = 0;
                        continue;
                    }
                    else
                    {
                        newStr[newStr.Length - 1 - i] = target[target.Length - 1 - j];
                        j++;
                        k++;
                        continue;
                    }
                }
                return new string(newStr);
            }
            else { return new string(target); }

        }
    }
}

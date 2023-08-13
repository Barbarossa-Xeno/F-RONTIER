/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Game;

namespace FancyScrollView.SongSelect
{
    public class ScrollManager : MonoBehaviour
    {
        [SerializeField] ScrollView scrollView = default;
        [SerializeField] Button prevCellButton = default;
        [SerializeField] Button nextCellButton = default;
        [SerializeField] Text selectedItemInfo = default;
        [HideInInspector] public int AmountOfElementsRange;
        [HideInInspector] public SongList songList;
        [HideInInspector] public int difficulty;
        private AudioClip[] audioClips;
        private ItemData[] items;
        private int _index;

        void Start()
        {
            audioClips = new AudioClip[AmountOfElementsRange];
            for (int i = 0; i < audioClips.Length; i++)
            {
                audioClips[i] = (AudioClip)Resources.Load<AudioClip>($"Data/{i}/highlight");
            }
            //prevCellButton.onClick.AddListener(scrollView.SelectPrevCell);
            //nextCellButton.onClick.AddListener(scrollView.SelectNextCell);
            scrollView.OnSelectionChanged(OnSelectionChangedAction);
            items = Enumerable.Range(0, AmountOfElementsRange)
                .Select(i => new ItemData(songList, i, difficulty))
                .ToArray();

            scrollView.UpdateData(items);
            scrollView.SelectCell(MenuInfo.menuInfo.selectedSongIndexInMenu);
        }

        void Update()
        {
            //Update内にもコンストラクタと同じ処理を書くことで難易度の変更に対応しました。
            items = Enumerable.Range(0, AmountOfElementsRange)
                .Select(i => new ItemData(songList, i, difficulty))  //difficultyは難易度変更ボタンが押されるごとに値が渡されます。
                .ToArray();

            scrollView.UpdateData(items);
            Game.Development.DevelopmentExtentionMethods.Log(_index);
            MenuInfo.menuInfo.selectedSongLevel = items[_index].Level;
        }

        ///<summary>現在選択されているセルに基づいて処理を行うメソッドです。</summary>
        ///<remarks>※<see cref = "FancyScrollView.SongSelect.ScrollView"/>組み込みメソッド<see cref = "FancyScrollView.Scroller.OnSelectionChanged(System.Action{int})"/>のコールバックに指定する。</remarks>
        void OnSelectionChangedAction(int index)
        {
            //selectedItemInfo.text = $"Selected item info: index {index}";
            _index = index;
            GameManager.instance.musicSource.clip = audioClips[index];
            GameManager.instance.musicSource.Play();
            MenuInfo.menuInfo.selectedSongIndexInMenu = index;
            MenuInfo.menuInfo.selectedSongID = items[index].SongID;
            MenuInfo.menuInfo.selectedSongTitle = items[index].Title;
            MenuInfo.menuInfo.selectedDifficulty = MenuInfo.menuInfo.DifficultyValueToRank(items[index].Difficulty);
            MenuInfo.menuInfo.selectedSongLevel = items[index].Level;
            MenuInfo.menuInfo.selectedImageColor = MenuInfo.menuInfo.DifficultyToImageColor(items[index].Difficulty);
            MenuInfo.menuInfo.selectedSongCover = (Sprite)Resources.Load<Sprite>($"Data/{index}/cover");
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IngameDebugConsole
{
    public class DebugCommandItemListView : MonoBehaviour
    {
        // Cached components
        [SerializeField]
        private RectTransform transformComponent;
        [SerializeField]
        private RectTransform viewportTransform;

        [SerializeField]
        private DebugLogManager debugManager;

        [SerializeField]
        private Color itemNormalColor1;
        [SerializeField]
        private Color itemNormalColor2;
        [SerializeField]
        private Color itemSelectedColor;
        [SerializeField]
        private DebugCommandItem itemPrefab;
        [SerializeField]
        private float itemHeight;

        private int currentTopIndex = -1, currentBottomIndex = -1;
        public float ItemHeight { get { return itemHeight; } }
        private float viewportHeight;
        private string curCommand;
        private bool showFlag;

        private Dictionary<int, DebugCommandItem> itemAtIndex = new Dictionary<int, DebugCommandItem>();
        private List<DebugCommandItem> pooledItems = new List<DebugCommandItem>();

        private Dictionary<string, ConsoleMethodInfo> methodInfoDic = null;  //all
        private List<ConsoleMethodInfo> tempMethodInfoList = new List<ConsoleMethodInfo>();

        public void Initialize(DebugLogManager manager, Dictionary<string, ConsoleMethodInfo> methodInfos)
        {
            viewportHeight = viewportTransform.rect.height;
            this.debugManager = manager;
            this.methodInfoDic = methodInfos;
            foreach(var keyvalue in methodInfoDic)
            {
                tempMethodInfoList.Add(keyvalue.Value);
            }
        }

        public void Show(bool flag, string command)
        {
            showFlag = flag;
            if(flag && curCommand != ParseCommand(command))
            {
                SetCommand(curCommand);
            }
        }

        public void OnItemClicked(DebugCommandItem item)
        {
            debugManager.SetCommand(item.MethodInfo.command);
        }

        public void SetCommand(string command)
        {
            if (showFlag)
            {
                curCommand = ParseCommand(command);

                SearchCommand(curCommand);
                viewportHeight = viewportTransform.rect.height;
                CalculateContentHeight();

                HardResetItems();
                UpdateItemsInTheList(true);
            }
        }

        private string ParseCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return command;
            }
            else
            {
                command = command.Trim();
                int index = command.IndexOf(' ', 0);
                if (index < 0) index = command.Length;
                return command.Substring(0, index);
            }
        }

        private void SearchCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                tempMethodInfoList.Clear();
                foreach (var keyvalue in methodInfoDic)
                {
                    tempMethodInfoList.Add(keyvalue.Value);
                }
            }
            /*
            else if (add && tempMethodInfoList.Count > 0)
            {
                for (int i = tempMethodInfoList.Count - 1; i >= 0; i--)
                {
                    if (!tempMethodInfoList[i].command.Contains(command))
                    {
                        tempMethodInfoList.RemoveAt(i);
                    }
                }
            }
            */
            else
            {
                tempMethodInfoList.Clear();
                foreach (var keyvalue in methodInfoDic)
                {
                    if (keyvalue.Key.Contains(command))
                        tempMethodInfoList.Add(keyvalue.Value);
                }
            }
        }

        //public void OnMethodUpdated(bool updateAllVisibleItemContents)
        //{
        //    SearchCommand(curCommand, false);
        //    CalculateContentHeight();
        //    viewportHeight = viewportTransform.rect.height;

        //    if (updateAllVisibleItemContents)
        //        HardResetItems();

        //    UpdateItemsInTheList(updateAllVisibleItemContents);
        //}

        public void OnViewportChanged()
        {
            viewportHeight = viewportTransform.rect.height;
            UpdateItemsInTheList(false);
        }

        // Calculate the indices of log entries to show
        // and handle log items accordingly
        private void UpdateItemsInTheList(bool updateAllVisibleItemContents)
        {
            if (tempMethodInfoList.Count > 0)
            {
                float contentPosTop = transformComponent.anchoredPosition.y - 1f;
                float contentPosBottom = contentPosTop + viewportHeight + 2f;

                int newTopIndex = (int)(contentPosTop / itemHeight);
                int newBottomIndex = (int)(contentPosBottom / itemHeight);

                if (newTopIndex < 0) newTopIndex = 0;
                if (newBottomIndex > tempMethodInfoList.Count - 1)
                    newBottomIndex = tempMethodInfoList.Count - 1;

                if (currentTopIndex == -1)
                {
                    updateAllVisibleItemContents = true;

                    currentTopIndex = newTopIndex;
                    currentBottomIndex = newBottomIndex;

                    CreateItemsBetween(newTopIndex, newBottomIndex);
                }
                else
                {
                    // There are some log items visible on screen
                    if (newBottomIndex < currentTopIndex || newTopIndex > currentBottomIndex)
                    {
                        // If user scrolled a lot such that, none of the log items are now within
                        // the bounds of the scroll view, pool all the previous log items and create
                        // new log items for the new list of visible debug entries
                        updateAllVisibleItemContents = true;

                        DestroyItemsBetween(currentTopIndex, currentBottomIndex);
                        CreateItemsBetween(newTopIndex, newBottomIndex);
                    }
                    else
                    {
                        // User did not scroll a lot such that, there are still some log items within
                        // the bounds of the scroll view. Don't destroy them but update their content,
                        // if necessary
                        if (newTopIndex > currentTopIndex)
                            DestroyItemsBetween(currentTopIndex, newTopIndex - 1);

                        if (newBottomIndex < currentBottomIndex)
                            DestroyItemsBetween(newBottomIndex + 1, currentBottomIndex);

                        if (newTopIndex < currentTopIndex)
                        {
                            CreateItemsBetween(newTopIndex, currentTopIndex - 1);

                            // If it is not necessary to update all the log items,
                            // then just update the newly created log items. Otherwise,
                            // wait for the major update
                            if (!updateAllVisibleItemContents)
                                UpdateItemsBetween(newTopIndex, currentTopIndex - 1);
                        }

                        if (newBottomIndex > currentBottomIndex)
                        {
                            CreateItemsBetween(currentBottomIndex + 1, newBottomIndex);

                            // If it is not necessary to update all the log items,
                            // then just update the newly created log items. Otherwise,
                            // wait for the major update
                            if (!updateAllVisibleItemContents)
                                UpdateItemsBetween(currentBottomIndex + 1, newBottomIndex);
                        }
                    }

                    currentTopIndex = newTopIndex;
                    currentBottomIndex = newBottomIndex;
                }

                if (updateAllVisibleItemContents)
                {
                    // Update all the log items
                    UpdateItemsBetween(currentTopIndex, currentBottomIndex);
                }
            }
            else
                HardResetItems();
        }

        private void CalculateContentHeight()
        {
            float newHeight = Mathf.Max(1f, tempMethodInfoList.Count * itemHeight);
            transformComponent.sizeDelta = new Vector2(0f, newHeight);
            if(newHeight <= viewportHeight)
            {
                var anchoredPosition = transformComponent.anchoredPosition;
                anchoredPosition.y = 0;
                transformComponent.anchoredPosition = anchoredPosition;
            }
        }

        private void HardResetItems()
        {
            if (currentTopIndex != -1)
            {
                DestroyItemsBetween(currentTopIndex, currentBottomIndex);
                currentTopIndex = -1;
            }
        }

        private void DestroyItemsBetween(int topIndex, int bottomIndex)
        {
            for (int i = topIndex; i <= bottomIndex; i++)
                PoolItem(itemAtIndex[i]);
        }

        private void CreateItemsBetween(int topIndex, int bottomIndex)
        {
            for (int i = topIndex; i <= bottomIndex; i++)
                CreateItemAt(i);
        }

        private void UpdateItemsBetween(int topIndex, int bottomIndex)
        {
            for (int i = topIndex; i <= bottomIndex; i++)
            {
                var item = itemAtIndex[i];
                item.SetContent(tempMethodInfoList[i], i);
            }
        }

        private void CreateItemAt(int index)
        {
            var item = PopItem();
            // Reposition the log item
            Vector2 anchoredPosition = new Vector2(1f, -index * itemHeight);
            item.Transform.anchoredPosition = anchoredPosition;
            ColorItem(item, index);
            // To access this log item easily in the future, add it to the dictionary
            itemAtIndex[index] = item;
        }



        private void ColorItem(DebugCommandItem item, int index)
        {
            if (index % 2 == 0)
                item.Image.color = itemNormalColor1;
            else
                item.Image.color = itemNormalColor2;
        }

        #region pool
        // Pool an unused log item
        private void PoolItem(DebugCommandItem item)
        {
            item.gameObject.SetActive(false);
            pooledItems.Add(item);
        }


        // Fetch a log item from the pool
        private DebugCommandItem PopItem()
        {
            DebugCommandItem newLogItem;
            if (pooledItems.Count > 0)
            {
                newLogItem = pooledItems[pooledItems.Count - 1];
                pooledItems.RemoveAt(pooledItems.Count - 1);
                newLogItem.gameObject.SetActive(true);
            }
            else
            {
                newLogItem = Instantiate(itemPrefab, transformComponent, false);
                newLogItem.Initialize(this);
            }

            return newLogItem;
        }
        #endregion
    }
}

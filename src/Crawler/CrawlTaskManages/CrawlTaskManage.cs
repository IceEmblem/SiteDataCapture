using Crawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.CrawlTaskManages
{
    public class CrawlTaskManage
    {
        private int _currentThreadCount { get; set; }

        private int _maxThreadCount { get; set; }

        private Queue<ICrawlTask> _taskQueue { get; set; }

        private object _taskQueueLock { get; } = new object();

        public CrawlTaskManage(int MaxThreadCount)
        {
            _maxThreadCount = MaxThreadCount;
        }

        /// <summary>
        /// 启动新任务
        /// </summary>
        public void AddNewTask(ICrawlTask task)
        {
            _taskQueue.Enqueue(task);
        } 

        public void Start()
        {
            while (_currentThreadCount > 0) {
                if (_currentThreadCount < _maxThreadCount && _taskQueue.Count > 0)
                {
                    _currentThreadCount++;
                    StartNewTask();
                }
                else 
                {
                    Thread.Sleep(2000);
                }
            }
        }

        private void StartNewTask() 
        {
            ICrawlTask task = null;

            lock (_taskQueueLock)
            {
                if (_taskQueue.Count == 0)
                {
                    _currentThreadCount--;
                    return;
                }

                task = _taskQueue.Dequeue();
            }

            task.Run().ContinueWith((Task<IEnumerable<ICrawlTask>> tasks) => {
                foreach (var t in tasks.Result)
                {
                    _taskQueue.Enqueue(t);
                }

                StartNewTask();
            });
        }
    }
}

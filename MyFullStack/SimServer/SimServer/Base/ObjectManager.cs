using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    //类对象池
#if UNITY_EDITOR&&OBJ_POOL_TRACE_STACK
    public class ClassObjectPool<T>:ClassPoolBase where T : class, new()
#else
    public class ClassObjectPool<T> where T : class, new()
#endif
    {
        //池
        protected Stack<T> mPool = new Stack<T>();
        //最大对象个数，<=0表示不限
        protected int miMaxCount;
        //没有回收的对象个数
        protected int m_nNoRecycleCount = 0;

#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
    protected bool m_bHavePromptPoolIsEmpty = false;//是否提示过池已经空了，只提示一次
    protected Dictionary<T, string> m_NoRecycleItem = new Dictionary<T, string>();
    //打印日志
    protected override void PrintLog()
    {
        if (m_nNoRecycleCount > 0)
        {
            string msg = "从类对象池申请的对象没有全部回收，有泄漏可能性，请检查：" + GetType().ToString() + ",未回收个数" + m_nNoRecycleCount + ",申请调用堆栈如下：\r\n";
            Dictionary<T, string>.Enumerator it = m_NoRecycleItem.GetEnumerator();
            int i = 1;
            while (it.MoveNext())
            {
                msg += "第" + (i++) + "次：\r\n" + it.Current.Value;
                if (i > 10)
                {
                    break;
                }
            }

            GDebugger.LogError(msg);
        }
    }
#endif

        //iNewCountPercent,开始初始化iMaxCount的百分之多少个
        public ClassObjectPool(int iMaxCount)
        {
            miMaxCount = iMaxCount;
            for (int i = 0; i < iMaxCount; i++)
            {
                mPool.Push(new T());
            }

#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
        m_PoolList.Add(this);
#endif
        }

        //扩大池
        public void Enlarge(int iExpandCount)
        {
            if (miMaxCount >= 0)
            {
                miMaxCount = m_nNoRecycleCount = mPool.Count + iExpandCount;
            }

            for (int i = 0; i < iExpandCount; i++)
            {
                mPool.Push(new T());
            }
        }

        //产生一个新对象
        public T Spawn(bool bCreateIfPoolEmpty = true)//bCreateIfPoolEmpty如果pool为空时是否new新的出来
        {
            if (mPool.Count > 0)
            {
                T rtn = mPool.Pop();
                if (rtn == null)
                {
#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
                GDebugger.LogError("Spawn is null");
#endif
                    if (bCreateIfPoolEmpty)
                    {
                        rtn = new T();
                    }
                }
                m_nNoRecycleCount++;
#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
            m_NoRecycleItem.Add(rtn, StackTraceUtility.ExtractStackTrace());
#endif
                return rtn;
            }

#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
        if (miMaxCount > 0 && !m_bHavePromptPoolIsEmpty)
        {
            m_bHavePromptPoolIsEmpty = true;
            GDebugger.LogError("对象池已经空了，是否初始池太小或者没有完全回收?当前设定大小为：" + miMaxCount + "," + GetType().ToString());
        }
#endif

            if (bCreateIfPoolEmpty)
            {
                T rtn = new T();
                m_nNoRecycleCount++;
#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
            m_NoRecycleItem.Add(rtn, StackTraceUtility.ExtractStackTrace());
#endif
                return rtn;
            }

            return null;
        }
        //回收一个对象到对象池
        public bool Recycle(T obj)
        {
            if (obj == null)
            {
                return false;
            }

            m_nNoRecycleCount--;
#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
        m_NoRecycleItem.Remove(obj);
#endif

            if (mPool.Count >= miMaxCount && miMaxCount > 0)
            {
                obj = null;
                return false;
            }

#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
        if (mPool.Contains(obj))
        {
            GDebugger.LogError("同一个对象被回收了多次，请检查");
        }
        else
#endif
            {
                mPool.Push(obj);
            }
            return true;
        }
    }

#if UNITY_EDITOR && OBJ_POOL_TRACE_STACK
public class ClassPoolBase
{
    protected virtual void PrintLog() { }
    protected static List<ClassPoolBase> m_PoolList = new List<ClassPoolBase>();
    //析构函数
    public static void CheckRecycle()
    {
        int iCount = m_PoolList.Count;
        for (int i = 0; i < iCount; i++)
        {
            ClassPoolBase item = m_PoolList[i];
            item.PrintLog();
        }
    }
}
#endif

    class ObjectManager :Singleton<ObjectManager>
    {
        //类对象池列表
        protected static Dictionary<System.Type, object> m_dClassPool = new Dictionary<System.Type, object>();

        /// <summary>
        /// 创建类对象池,创建完以后可以外面保存WooolObjectPool<T>,然后调用Spawn和Recycle来创建和回收类对象
        /// </summary>
        /// <typeparam name="T">只要是类</typeparam>
        /// <param name="iMaxCount">最大个数</param>
        /// <returns></returns>
        public static ClassObjectPool<T> GetOrCreateClassPool<T>(int iMaxCount) where T : class, new()
        {
            System.Type theType = typeof(T);
            object outObj = null;
            if (!m_dClassPool.TryGetValue(theType, out outObj) || outObj == null)
            {
                ClassObjectPool<T> newPool = new ClassObjectPool<T>(iMaxCount);
                m_dClassPool.Add(theType, newPool);
                return newPool;
            }

            return outObj as ClassObjectPool<T>;
        }
    }

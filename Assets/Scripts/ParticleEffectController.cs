//粒子特效管理腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectController : MonoBehaviour
{
    private static ParticleEffectController _instance; //單例物件
    public static ParticleEffectController Instance { get { return _instance; } } //取得單例物件

    [System.Serializable]
    public struct ParticleEffectTicket //特效預置物件
    {
        public ParticleEffectType effectName; //效果名稱
        public GameObject prefab; //帶有Particle System的預置體
    }

    [System.Serializable]
    public struct StaticParticleObject //靜態特效物件
    {
        public string effectName; //特效名稱
        public int sortingOrder; //顯示層級
        public GameObject effectGo; //帶有Particle System的遊戲物件(已存在or預置體)
        public Transform parentHolder; //父物件

        //建構子
        public StaticParticleObject(string name, int layer, GameObject go, Transform parent)
        {
            effectName = name;
            sortingOrder = layer;
            effectGo = go;
            parentHolder = parent;
        }

        //參考物件取代
        public void ReplacePrefab(GameObject go)
        {
            effectGo = go;
        }
    }

    [Header("可自訂參數")]
    public List<ParticleEffectTicket> particleEffectList; //粒子特效列表
    public List<StaticParticleObject> staticEffectSetting; //靜態粒子特效設定

    [Header("參考物件")]
    public RectTransform parentRect; //父物件區域(Rect)

    private List<List<GameObject>> particleEffectPool = new List<List<GameObject>>(); //物件池(不同的ParticleEffectType各有一串物件池List)

    public Dictionary<ParticleEffectType, GameObject> dic_effectPrefab { private set; get; } //(字典)從ParticleEffectType取得Prefab(GameObject)
    public Dictionary<ParticleEffectType, int> dic_effectTypeIndex { private set; get; } //(字典)從ParticleEffectType取得物件池所在的陣列索引

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Start()
    {
        Initialize();
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    public void Initialize()
    {
        dic_effectPrefab = new Dictionary<ParticleEffectType, GameObject>();
        dic_effectTypeIndex = new Dictionary<ParticleEffectType, int>();

        if (particleEffectList == null) particleEffectList = new List<ParticleEffectTicket>();

        for (int i = 0; i < particleEffectList.Count; i++) //建立字典 : 從ParticleEffectType取得Prefab(GameObject)
        {
            dic_effectPrefab.Add(particleEffectList[i].effectName, particleEffectList[i].prefab);
        }
    }

    //粒子特效演出(多載1/2) ※一般特效撥放
    //[input] type = 特效種類 , pos = 位置, isWorld = true:世界座標/false:本地座標
    public void OneShotEffect(ParticleEffectType type, Vector2 pos, bool isWorld)
    {
        OneShotEffect(type, pos, isWorld, null);
    }

    //粒子特效演出(多載2/2) ※額外參數
    //[input] type = 特效種類 , pos = 位置, isWorld = true:世界座標/false:本地座標
    public void OneShotEffect(ParticleEffectType type, Vector2 pos, bool isWorld, object param)
    {
        GameObject go = ExtractFromPool(type); //從物件池中取得物件

        if (isWorld) go.transform.position = pos; //指定特效發生位置
        else go.transform.localPosition = pos; //指定特效發生位置

        //尋找粒子特效物件是否有額外行為
        foreach (IExtensibleParticle i in go.GetComponentsInChildren<IExtensibleParticle>())
        {
            i.Initialize(param);
        }

        foreach (ParticleSystemRenderer renderer in go.GetComponentsInChildren<ParticleSystemRenderer>()) //設定顯示層級
        {
            renderer.sortingOrder = parentRect.GetComponentInParent<Canvas>().sortingOrder;
        }

        go.GetComponent<ParticleSystem>().Play(); //執行特效
    }

    //設定靜態特效
    public void SetStaticEffect(string effectName, bool onOff)
    {
        StaticParticleObject _sta = new StaticParticleObject();
        int index = 0; //指定特效索引

        //取得指定名稱的特效
        for (int i = 0; i < staticEffectSetting.Count; i++)
        {
            if (staticEffectSetting[i].effectName == effectName)
            {
                _sta = staticEffectSetting[i];
                index = i;
                break;
            }

            if (i == staticEffectSetting.Count - 1) return; //搜尋不到結果時, 直接結束程序
        }

        if (_sta.effectGo == null) return; //未設定物件參考時, 直接結束程序

        foreach (ParticleSystemRenderer renderer in _sta.effectGo.GetComponentsInChildren<ParticleSystemRenderer>()) //設定顯示層級
        {
            renderer.sortingOrder = _sta.sortingOrder;
        }

        //開關特效Lambda方法
        System.Action<bool, ParticleSystem> SetEffectState = (bool b, ParticleSystem p) =>
        {
            if (b) //開啟特效
            {
                var parMain = p.main;
                parMain.loop = true;
                p.Play();
            }
            else //關閉特效
            {
                var parMain = p.main;
                parMain.loop = false;
            }
        };

        //特效行為邏輯實現
        ParticleSystem ps;

        if (!_sta.effectGo.activeInHierarchy) //物件不存在在編輯器上
        {
            GameObject insGo = Instantiate(_sta.effectGo, _sta.parentHolder); //創建物件

            _sta.ReplacePrefab(insGo); //以實例物件取代預置體
            staticEffectSetting[index] = _sta; //更新設定

            ps = insGo.GetComponent<ParticleSystem>();
            SetEffectState(onOff, ps);
        }
        else //正常執行開啟程序
        {
            ps = _sta.effectGo.GetComponent<ParticleSystem>();

            SetEffectState(onOff, ps);
        }

    }

    //加入靜態特效至設定
    public void AddStaticEffect(string effectName, GameObject prefab, Transform parent)
    {
        Canvas parentCanvas = parent.GetComponentInParent<Canvas>();

        //建立新的靜態特效設定
        StaticParticleObject _sta = new StaticParticleObject(effectName, parentCanvas.sortingOrder, prefab, parent);

        for (int i = 0; i < staticEffectSetting.Count; i++)
        {
            if (staticEffectSetting[i].effectName == effectName) //若已經存在相同名稱的設定
            {
                staticEffectSetting[i] = _sta; //取代原有設定
                return;
            }
        }

        staticEffectSetting.Add(_sta); //追加至設定列表
    }

    //從物件池中抽取物件
    private GameObject ExtractFromPool(ParticleEffectType effectType)
    {
        if (!dic_effectTypeIndex.ContainsKey(effectType)) //若指定的粒子特效尚未存在物件池
        {
            int _index = particleEffectPool.Count; //註冊一個索引值給預備建立的特效種類
            dic_effectTypeIndex.Add(effectType, _index); //建立字典儲存 ParticleEffectType ⇔ 索引值

            particleEffectPool.Add(new List<GameObject>()); //拓寬物件池, 用以儲存新的特效種類物件池
        }

        GameObject go = null;

        UnityEngine.Events.UnityAction CreateNew = new UnityEngine.Events.UnityAction(() => //Lambda:創立新物件&加長物件池陣列
        {
            go = Instantiate(dic_effectPrefab[effectType], parentRect); //建立新物件
            go.name += "_" + ( particleEffectPool[dic_effectTypeIndex[effectType]].Count + 1 ).ToString();
            particleEffectPool[dic_effectTypeIndex[effectType]].Add(go);
        });

        if (particleEffectPool[dic_effectTypeIndex[effectType]].Count == 0) CreateNew(); //若物件池中沒有任何物件
        else //若物件中存在物件
        {
            int index = -1; //物件池陣列索引
            for (int i = 0; i < particleEffectPool[dic_effectTypeIndex[effectType]].Count; i++)
            {
                if (!particleEffectPool[dic_effectTypeIndex[effectType]][i].GetComponent<ParticleSystem>().isPlaying) //若物件池中有找到未激活(!isPlaying)物件, 則設定其物件所在索引
                {
                    index = i;
                    break;
                }
            }

            if (index < 0) CreateNew(); //若物件池中的物件都在激活狀態, 則創立新物件
            else go = particleEffectPool[dic_effectTypeIndex[effectType]][index]; //若物件池中有物件處於未激活(!isPlaying), 則設定回傳該物件
        }

        return go;
    }
}

//粒子特效種類
public enum ParticleEffectType
{
    球互撞, 打擊正面方塊, 打擊負面方塊, 條件倒轉
}
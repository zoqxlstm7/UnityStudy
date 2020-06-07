# 유니티 게임 데이터 저장방법
## 저장 방법
### PlayerPrefs
- 유니티 PlayerPrefs 클래스 이용
- Windows의 경우 레지스트리에 값 저장
- 간단한 데이터 저장에 적합
#### 장단점
- 장점
    - 매우 간단하게 사용가능
- 단점
    - 저장 데이터타입과 용량 제한
        - float, int, string만 가능
        - wep의 경우 1MB로 제한
#### 사용법
- 게임 실행중에는 내부적으로 가지고 있다가 어플리케이션이 종료될 때 OnApplicationQuit() 이벤트에서 최종적으로 저장됨
```c#
// 키값을 사용한 저장
PlayerPrefs.SetInt("키값", 10);
PlayerPrefs.SetFloat("키값", 10.0f);
PlayerPrefs.SetString("키값", "저장데이터");
// 키값을 사용한 호출
PlayerPrefs.GetInt("저장할때 사용한 키값");
PlayerPrefs.GetFloat("저장할때 사용한 키값");
PlayerPrefs.GetString("저장할때 사용한 키값");
// 모든 데이터 저장
PlayerPrefs.Save();
// 데이터 삭제
PlayerPrefs.DeleteKey("키값");
PlayerPrefs.DeleteAll();
// 키값 확인
PlayerPrefs.HasKey("키값");
```

### Unity Serialization
- 유니티 Serialization API 이용
- 값을 binary 형태의 스트림으로 변환하여 파일에 저장 가능
#### 장단점
- 장점
    - 속도, 퍼포먼스 양호
- 단점
    - 저장되는 내용을 확인할 수 없음
    - 직렬화 가능한 데이터 타입만 가능
    - 저장필드가 변경되는 경우 처리가 까다로움
#### 스크립트에서 직렬화를 하려면?
- public 또는 [SerializeField] 속성이어야 한다
- static이 아닌 것
- const가 아닌 것
- readonly가 아닌 것
- fieldtype은 직렬화 할 수 있는 타입이어야 한다
##### 어떤 fieldtype을 직렬화할 수 있는가?
- [SerializeField] 속성을 가진 사용자 정의 비 추상 클래스
- [SerializeField] 속성을 가진 사용자 정의 구조체
- UnityEngine.Object에서 파생한 오브젝트
- 기본 데이터 타입 (int, float, double, bool, string, etc)
- 직렬화 가능한 필드 유형의 배열
- 직렬화 가능한 필드 유형의 리스트
#### 키워드
- System.Serialization, [SerializeField]
- Binaryformatter, FileStream, Serialize, Deserialize

> using System.Runtime.Serialization.Formatters.Binary;  
> using System.IO;

```c#
// 사용자 정의 클래스
[System.Serializable]
public class PlayerData
{
    public string name;
    public int level;
    public float playTime;

    public PlayerData(string name) => this.name = name;
}
// 쓰기
using (FileStream fsW = new FileStream(Application.persistentDataPath + "UnitySerialization.dat", FileMode.OpenOrCreate))
{
    BinaryFormatter bf = new BinaryFormatter();
    bf.Serialize(fsW, playerData);
}
// 읽기
using(FileStream fwR = new FileStream(Application.persistentDataPath + "UnitySerialization.dat", FileMode.Open))
{
    BinaryFormatter bf = new BinaryFormatter();
    playerData = bf.Deserialize(fwR) as PlayerData;
}
```

### XML Serialization
- C# XML Serialization 클래스 이용
- 데이터를 XML구조의 xml파일로 저장
- user data보다는 게임 인벤토리 데이터 저장에 유용
#### 장단점
- 장점
    - 저장 데이터의 가독성이 우수
-단점
    - 속도가 Unity Serialization보다 느림
    - 저장파일을 열어 편집 가능
#### 키워드
- 노드 구성
    - [XmlRoot("루트명")]
    - [XmlArray("어레이명")]
    - [XmlArrayItem("아이템명")]
    - [XmlAttribute("어트리뷰트명")]
- XmlSerializer, FileStream, StreamWriter, Serialize, Deserialize
> using System.Xml;  
> using System.Xml.Serialization;
```c#
// 사용자 정의 클래스
public class PlayerDataForXML
{
    [XmlAttribute("name")]
    public string name = "xml 저장";
    public int level;
    public float playTime;
}
// 컨테이너 클래스와 배열
[XmlRoot("PlayerDataCollection")]
public class PlayerDataContainer
{
    [XmlArray("PlayerDatas"), XmlArrayItem("PlayerData")]
    public List<PlayerDataForXML> playerDatas = new List<PlayerDataForXML>();
}
// 쓰기
using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate), Encoding.UTF8))
{
    XmlSerializer serializer = new XmlSerializer(typeof(PlayerDataContainer));
    serializer.Serialize(sw, this);
}
// 읽기
using(FileStream fwR = new FileStream(path, FileMode.Open))
{
    XmlSerializer serializer = new XmlSerializer(typeof(PlayerDataContainer));
    return serializer.Deserialize(fwR) as PlayerDataContainer;
}
```
#### 한글 OS에서의 문제
- 에러 메세지
    - Encoding name 'ks_c_5601-1987' not supported
- 한글 OS에서 파일스트림으로 xml파일을 생성할 때 발생하는 문제
- StreamWirter를 통해 Encoding.UTF8로 인코딩을 정해주면 해결된다

### JSON Serialization
- 유니티 엔진의 JsonUtility 클래스 이용
- UnityEngin 지시문에 포함되어 있으므로 따로 지시문을 넣을 필요가 없다
- 클래스, 데이터를 JSON 형식의 문자열로 변환하여 저장하거나 가져올 수 있는 기능
- 기존에는 별도의 플러그인이 필요했으나 5.3버전 이상부터는 기본 API로 제공
- 웹서비스 연동 시 유용
#### 장단점
- 장점
    - 저장 데이터의 가독성이 좋음
    - 디버깅에 용이
    - 배열, 객체안의 객체를 넣는 것도 가능
    - 정수, 실수, 문자열, bool형, null 타입의 데이터 타입 지원
- 단점
    - 많은 양의 Json 데이터의 경우 퍼포먼스가 저하될 수 있다
    - 문법 오류에 민감하다
#### 유니티의 JsonUtility가 제공하는 특수한 기능
- Vector3 시리얼라이즈
    - 불필요한 값 없이 저장 가능 (x,y,z)
    - 다른 Json 라이브러리를 사용하면 normalized 프로퍼티로 인해 Self reference loop 문제가 발생하며, 이를 해결한다고 해도 정규화된 벡터와 그 길이, 길이의 제곱등의 불필요한 데이터가 저장됨
- MonoBehaviour를 상속하는 클래스의 오브젝트 시리얼라이즈
    - 해당 오브젝트가 아닌 GetComponent()등으로 직접 가져온 클래스를 시리얼라이즈할 것
    - 디시리얼라이즈할 때는 From.JsonOverwirte() 함수를 사용할 것
        - JsonOverwirte(): JSON데이터를 오브젝트로 변환할 때, 새로운 오브젝트를 만들지 않고 기존에 있는 오브젝트에 클래스의 변수값을 덮어씌우는 처리를 한다.
- JsonUtility는 Dictionary를 지원하지 않음
#### 키워드
- JsonUtility.ToJson()
- JsonUtility.FromJson()
- JsonOverwirte()
- FileStream, StreamWrite, StreamReader
```c#
// 사용자 정의 클래스
[System.Serializable]
public class PlayerData
{
    public string name;
    public int level;
    public float playTime;

    public PlayerData(string name) => this.name = name;
}
// 쓰기
using(StreamWriter sw = new StreamWriter(new FileStream(DATA_PATH, FileMode.OpenOrCreate), Encoding.UTF8))
{
    string json = JsonUtility.ToJson(playerData);
    sw.Write(json);
}
// 읽기
using(StreamReader sr = new StreamReader(new FileStream(DATA_PATH, FileMode.Open), Encoding.UTF8))
{
    string json = sr.ReadToEnd();
    playerData = JsonUtility.FromJson<PlayerData>(json);
}
```

### ScripableObject
- 유니티 Asset에 저장되는 데이터
- 데이터 저장 목적의 스크립트 오브젝트 5.3이상의 버전에서 제공
- 게임 기초 데이터나 인벤토리 데이터 저장에 유용
- AssetDatabase 활용
#### 장단점
- 장점
    - 실제 모든 오브젝트를 가지고 있는 것이 아닌 reference만 가지고 있기 때문에 속도, 퍼포먼스 우수
- 단점
    - 게임 중 발생/변경된 데이터를 저장하는 것에 적합하지 않음
#### 키워드
- ScriptableObject
- ScriptableObject.CreateInstance< T >()
- AssetDatabase.CreateAsset(), AssetDatabase.SaveAssets()

> using UnityEditor;

```c#
// Scriptable Object
public class PlayerDataForScriptable : ScriptableObject
{
    public PlayerData playerData;
}
// 에셋데이터베이스 생성
saveData = ScriptableObject.CreateInstance<PlayerDataForScriptable>();

AssetDatabase.CreateAsset(saveData, "Assets/Resources/Data/AssetData.asset");
AssetDatabase.SaveAssets();
// 읽기
saveData = Resources.Load<PlayerDataForScriptable>("Data/AssetData");
playerData = saveData.playerData;
```

### RDBMS (관계형 데이터베이스)
- mySql 등의 데이터베이스 활용
#### 장단점
- 장점
    - 안정적인 데이터 관리
- 단점
    - 서버 구축 필요
    - 데이터 관리 필요

### 몽고디비 (NoSql)
- 비정형
- 비관계형
- 문서 기반
- NoSQL
#### 주요특징
- JOIN 불가
- 테이블간 관계 없음
- 필드에 인덱스 추가 가능 (필드의 값은 배열이 될 수 있다.)
#### 장점
- 장점
    - 비정형 데이터 관리
    - JSON형식의 데이터
    - 손쉬운 오브젝트 할당
    - ObjectID로 조회시 속도가 빠름
#### 개념
- collection: Table
- document: Row
- field: Column
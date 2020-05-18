using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NetworkConnectionInfo
{
    public bool host;       // 호스트로 실행 여부
    public string ipAdress; // 클라이언트로 실행 시 접속할 호스트의 IP 주소
    public int port;        // 클라이언트로 실행 시 접속할 호스트의 Port
}

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static FRONTIER.Utility.Reference;

namespace FRONTIER.Game.Notes
{
    // 注意書き：
    // - このクラスは分割定義されており、このクラスでは継承されたメソッド以外の追加分を定義しています。
    //   フィールドや継承メソッドについては、<c>LongNotesManager.cs</c>で定義しています。
    public partial class LongNotesManager
    {
        #region セットアップ系メソッド

        /// <summary>
        /// 1まとまりごとにロングノーツの帯を作ります。
        /// </summary>
        /// <remarks>
        /// ロングノーツの帯となるオブジェクトの位置や、最後に <see cref="SetRibbonMesh"/> を呼び出してメッシュを設定する。
        /// </remarks>
        ///<param name = "startLane">ロングノーツの始点が置かれるレーン番号。</param>
        ///<param name = "startZ">ロングノーツの始点のZ座標。</param>
        ///<param name = "endLane">ロングノーツの終点が置かれるレーン番号。</param>
        ///<param name = "endZ">ロングノーツの終点のZ座標。</param>
        ///<param name = "type">ロングノーツの種類。<br/>中間点無しなら0、中間点有りなら1。</param>
        ///<param name = "index">譜面上で何番目のロングノーツか。</param>
        private void CreateRibbon(int startLane, float startZ, int endLane, float endZ, LongNoteType type, int index)
        {
            // ロングノーツの種類を大きく分けて、直線型か曲線型かにカテゴライズ
            NoteType categorizedType = type switch
            {
                LongNoteType.DirectLinear or LongNoteType.IntermediateLinear
                => NoteType.LinearLong,
                LongNoteType.DirectCurved or LongNoteType.IntermediateCurved
                => NoteType.CurvedLong,
                _ => NoteType.LinearLong
            };

            // 中間点の有無で true / false
            // is パターンマッチングで簡素化＋明確化
            bool hasIntermediate = type is LongNoteType.IntermediateLinear or LongNoteType.IntermediateCurved;

            // ロングノーツ線にコンポーネントを付与
            ribbon = new GameObject($"LongNoteRibbon-{index}").AddComponent<LongNote>();

            ribbon.gameObject.AddComponent<MeshRenderer>();
            ribbon.gameObject.AddComponent<MeshFilter>();
            ribbon.gameObject.AddComponent<MeshCollider>();
            ribbon.gameObject.tag = "LongNoteRibbon";
            ribbon.gameObject.layer = LayerMask.NameToLayer("LongNoteRibbon");
            
            ribbon.SetProperties(categorizedType, index, LongNotePart.Ribbon, hasIntermediate);

            ribbons.Add(ribbon);

            // レーン番号からX座標を求め、パラメーターを元にノーツの始点と終点座標を計算
            Vector3 start = new(LANE_GAP * RIBBON_WIDTH + startLane * RIBBON_WIDTH + RIBBON_WIDTH / 2, specialNoteOrigin.y, startZ);
            Vector3 end = new(LANE_GAP * RIBBON_WIDTH + endLane * RIBBON_WIDTH + RIBBON_WIDTH / 2, specialNoteOrigin.y, endZ);

            // 曲線型の場合、制御点も計算
            Vector3 anchor;

            // 曲線型の場合
            if (categorizedType == NoteType.CurvedLong)
            {
                // 制御点の計算
                anchor = new(LANE_GAP * RIBBON_WIDTH + endLane * RIBBON_WIDTH + RIBBON_WIDTH / 2, specialNoteOrigin.y, (startZ + endZ) / 2);

                // ベジェ曲線から計算した曲線上の点
                Vector3[] curves = CalculateBezierCurves(start, anchor, end, splitSize: SPLIT_SIZE);

                // 曲線を構成するオブジェクトの断片をつくる
                GameObject[] fragments = new GameObject[curves.Length - 1];
                for (int i = 0; i < fragments.Length; i++)
                {
                    fragments[i] = new GameObject($"SplittedRibbon-{i} (LongNoteRibbon-{index})");
                    fragments[i].AddComponent<MeshFilter>();
                    fragments[i].AddComponent<MeshRenderer>();
                    fragments[i].AddComponent<MeshCollider>();

                    // ロングノーツ１まとまりを、1つのゲームオブジェクトとして扱うために、ロングノーツ線オブジェクトの子とする
                    fragments[i].transform.SetParent(ribbon.transform);
                }

                // 断片を生成する
                for (int i = 0; i < fragments.Length; i++)
                {
                    // ロングノーツ1まとまりにおいて、最後の断片のとき isLast = true にすればよいので
                    SetRibbonMesh(curves[i], curves[i + 1], type, target: fragments[i], split: SPLIT_SIZE, anchors: curves, isLast: i == fragments.Length - 1);
                }
            }
            // 直線型の場合（曲線の座標も渡さない）
            else
            {
                SetRibbonMesh(start, end, type);
            }
        }

        /// <summary>
        /// 指定したロングノーツの帯に対して、適切なメッシュやUV、マテリアルを計算して設定します。指定がない場合 <see cref="ribbon" /> に設定します。
        /// </summary>
        /// <param name = "start">ロングノーツの始点座標。</param>
        /// <param name = "end">ロングノーツの終点座標。</param>
        /// <param name = "type">ロングノーツの種類。<br/>中間点無しなら0、中間点有りなら1。</param>
        /// <param name = "target">指定したロングノーツの帯のゲームオブジェクト。</param>
        /// <param name = "anchors">曲線を作る点の座標。</param>
        /// <param name = "split">曲線型ロングノーツの分割数。</param>
        /// <param name = "isLast">分割したロングノーツの生成時、その対象が最後の分割ノーツだったときtrueを指定する。</param>
        private void SetRibbonMesh(Vector3 start, Vector3 end, LongNoteType type, GameObject target = null, Vector3[] anchors = null, int split = 10, bool isLast = false)
        {
            // ターゲットがない場合デフォルトで ribbon を使用
            if (target == null)
            {
                target = ribbon.gameObject;
            }
            // MeshFilter のパラメータ
            MeshFilterParameters filterParams = new()
            {
                vertices = new Vector3[4 * 6],
                triangles = new int[6 * 6],
                uvs = new Vector2[4 * 6]
            };

            // MeshColliderのパラメータ
            MeshColliderParameters colliderParams = new()
            {
                vertices = new Vector3[4 * 6],
                triangles = new int[6 * 6]
            };

            // メッシュの頂点とUV座標、コライダーの座標を計算
            // 面1（ワールド原点からZ座標を正の方向に見たときの、上底 => Y方向）
            filterParams.vertices[0] = start + new Vector3(-RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);     // 始点の左端 = 左下
            filterParams.vertices[1] = start + new Vector3(RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);      // 始点の右端 = 右下
            filterParams.vertices[2] = end + new Vector3(-RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);       // 終点の左端 = 左上
            filterParams.vertices[3] = end + new Vector3(RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);        // 終点の右端 = 右上
            filterParams.triangles[0] = 0;
            filterParams.triangles[1] = 2;
            filterParams.triangles[2] = 1;
            filterParams.triangles[3] = 3;
            filterParams.triangles[4] = 1;
            filterParams.triangles[5] = 2;
            filterParams.uvs[0] = new Vector2(0, 0);
            filterParams.uvs[1] = new Vector2(1, 0);
            filterParams.uvs[2] = new Vector2(0, 1);
            filterParams.uvs[3] = new Vector2(1, 1);
            colliderParams.vertices[0] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[0];
            colliderParams.vertices[1] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[1];
            colliderParams.vertices[2] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[2];
            colliderParams.vertices[3] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[3];

            // 面2（前側面 => -Z方向）
            filterParams.vertices[4] = start + new Vector3(-RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);    // 左下
            filterParams.vertices[5] = start + new Vector3(RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);     // 右下
            filterParams.vertices[6] = start + new Vector3(-RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);     // 左上
            filterParams.vertices[7] = start + new Vector3(RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);      // 右上
            filterParams.triangles[6] = 4;
            filterParams.triangles[7] = 6;
            filterParams.triangles[8] = 5;
            filterParams.triangles[9] = 7;
            filterParams.triangles[10] = 5;
            filterParams.triangles[11] = 6;
            filterParams.uvs[4] = new Vector2(0, 0);
            filterParams.uvs[5] = new Vector2(1, 0);
            filterParams.uvs[6] = new Vector2(0, 1);
            filterParams.uvs[7] = new Vector2(1, 1);
            colliderParams.vertices[4] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[4];
            colliderParams.vertices[5] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[5];
            colliderParams.vertices[6] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[6];
            colliderParams.vertices[7] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[7];

            // 面3（左側面 => -X方向）
            filterParams.vertices[8] = end + new Vector3(-RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);      // 左下
            filterParams.vertices[9] = start + new Vector3(-RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);    // 右下
            filterParams.vertices[10] = end + new Vector3(-RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);      // 左上
            filterParams.vertices[11] = start + new Vector3(-RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);    // 右上
            filterParams.triangles[12] = 8;
            filterParams.triangles[13] = 10;
            filterParams.triangles[14] = 9;
            filterParams.triangles[15] = 11;
            filterParams.triangles[16] = 9;
            filterParams.triangles[17] = 10;
            filterParams.uvs[8] = new Vector2(0, 0);
            filterParams.uvs[9] = new Vector2(1, 0);
            filterParams.uvs[10] = new Vector2(0, 1);
            filterParams.uvs[11] = new Vector2(1, 1);
            colliderParams.vertices[8] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[8];
            colliderParams.vertices[9] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[9];
            colliderParams.vertices[10] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[10];
            colliderParams.vertices[11] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[11];

            // 面4（右側面 => X方向）
            filterParams.vertices[12] = start + new Vector3(RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);    // 左下
            filterParams.vertices[13] = end + new Vector3(RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);      // 右下
            filterParams.vertices[14] = start + new Vector3(RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);     // 左上
            filterParams.vertices[15] = end + new Vector3(RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);       // 右上
            filterParams.triangles[18] = 12;
            filterParams.triangles[19] = 14;
            filterParams.triangles[20] = 13;
            filterParams.triangles[21] = 15;
            filterParams.triangles[22] = 13;
            filterParams.triangles[23] = 14;
            filterParams.uvs[12] = new Vector2(0, 0);
            filterParams.uvs[13] = new Vector2(1, 0);
            filterParams.uvs[14] = new Vector2(0, 1);
            filterParams.uvs[15] = new Vector2(1, 1);
            colliderParams.vertices[12] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[12];
            colliderParams.vertices[13] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[13];
            colliderParams.vertices[14] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[14];
            colliderParams.vertices[15] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[15];

            // 面5（後側面 => Z方向）
            filterParams.vertices[16] = end + new Vector3(-RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);     // 左下
            filterParams.vertices[17] = end + new Vector3(RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);      // 右下
            filterParams.vertices[18] = end + new Vector3(-RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);      // 左上
            filterParams.vertices[19] = end + new Vector3(RIBBON_WIDTH / 2, RIBBON_HEIGHT / 2, 0);       // 右上
            filterParams.triangles[24] = 16;
            filterParams.triangles[25] = 17;
            filterParams.triangles[26] = 18;
            filterParams.triangles[27] = 19;
            filterParams.triangles[28] = 18;
            filterParams.triangles[29] = 17;
            filterParams.uvs[16] = new Vector2(0, 0);
            filterParams.uvs[17] = new Vector2(1, 0);
            filterParams.uvs[18] = new Vector2(0, 1);
            filterParams.uvs[19] = new Vector2(1, 1);
            colliderParams.vertices[16] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[16];
            colliderParams.vertices[17] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[17];
            colliderParams.vertices[18] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[18];
            colliderParams.vertices[19] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[19];

            // 面6（下底 => -Y方向）
            filterParams.vertices[20] = start + new Vector3(-RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);   // 始点の左端
            filterParams.vertices[21] = start + new Vector3(RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);    // 始点の右端
            filterParams.vertices[22] = end + new Vector3(-RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);     // 終点の左端
            filterParams.vertices[23] = end + new Vector3(RIBBON_WIDTH / 2, -RIBBON_HEIGHT / 2, 0);      // 終点の右端
            filterParams.triangles[30] = 20;
            filterParams.triangles[31] = 21;
            filterParams.triangles[32] = 22;
            filterParams.triangles[33] = 23;
            filterParams.triangles[34] = 22;
            filterParams.triangles[35] = 21;
            filterParams.uvs[20] = new Vector2(0, 0);
            filterParams.uvs[21] = new Vector2(1, 0);
            filterParams.uvs[22] = new Vector2(0, 1);
            filterParams.uvs[23] = new Vector2(1, 1);
            colliderParams.vertices[20] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[20];
            colliderParams.vertices[21] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[21];
            colliderParams.vertices[22] = new Vector3(-COLLIDER_WIDTH, 0, 0) + filterParams.vertices[22];
            colliderParams.vertices[23] = new Vector3(COLLIDER_WIDTH, 0, 0) + filterParams.vertices[23];

            colliderParams.triangles = filterParams.triangles;

            // 各コンポーネントに計算したメッシュを渡す
            target.GetComponent<MeshFilter>().mesh = filterParams.CreateMesh(); ;
            target.GetComponent<MeshCollider>().sharedMesh = colliderParams.CreateMesh();

            // コライダーを覆う場合は convex にチェックを入れる
            // target.GetComponent<MeshCollider>().convex = true;

            // 種類によって処理を分ける。
            switch (type)
            {
                case LongNoteType.DirectLinear:
                    {
                        target.GetComponent<MeshRenderer>().material = ribbonMaterials.direct;
                        break;
                    }
                // 中間点があればラインレンダラーで中心線を描画する
                case LongNoteType.IntermediateLinear:
                    {
                        Vector3[] linePositions = new Vector3[2] { start, end };

                        // ribbon に中心線描画のための LineRenderer を追加
                        LineRenderer lineRenderer = ribbon.gameObject.AddComponent<LineRenderer>();
                        lineRenderer.SetPositions(linePositions);
                        lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
                        lineRenderer.material = ribbonMaterials.line;
                        lineRenderer.useWorldSpace = false;

                        target.GetComponent<MeshRenderer>().material = ribbonMaterials.intermediate;
                        target.transform.SetParent(ribbon.transform);
                        break;
                    }
                case LongNoteType.DirectCurved:
                    {
                        // そのロングノーツのまとまりにおいて、最後となるノーツの断片を受け取ったら
                        if (isLast)
                        {
                            // メッシュの結合とテクスチャの再貼付
                            CombineFragmentMesh(ribbon.gameObject);
                            ReprintTexture(ribbon.gameObject, split, LongNoteType.DirectCurved);
                        }
                        break;
                    }
                case LongNoteType.IntermediateCurved:
                    {
                        // そのロングノーツのまとまりにおいて、最後となるノーツの断片を受け取ったら
                        // 曲線の座標を収めた配列がnullでないことを確認して
                        if (isLast && anchors != null)
                        {
                            // メッシュの結合とテクスチャの再貼付
                            CombineFragmentMesh(ribbon.gameObject);
                            ReprintTexture(ribbon.gameObject, split, LongNoteType.IntermediateCurved);

                            // ribbon に中心線描画のための LineRenderer を追加
                            LineRenderer lineRenderer = ribbon.gameObject.AddComponent<LineRenderer>();
                            lineRenderer.positionCount = anchors.Length;
                            lineRenderer.widthMultiplier = 0.1f;
                            lineRenderer.SetPositions(anchors);
                            lineRenderer.useWorldSpace = false;

                            lineRenderer.material = ribbonMaterials.line;
                            target.transform.SetParent(ribbon.transform);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// ロングノーツの帯の transform を適切に設定する
        /// 特に中間点有りのロングノーツについては、中間点どうしの間で各々生成されてしまうロングノーツの断片を、
        /// 譜面のロングノーツの順番ごとに1つの親オブジェクトに関連付けて、ロングノーツ１まとまりとする。
        /// </summary>
        private void SetRibbonTransform()
        {
            // 中間点なければ戻る
            if (ribbons.Count == 0)
            {
                return;
            }
            // 各ロングノーツに設定された、流れてくる順番 (LongNote.index) を抽出
            List<int> meshIndexList = ribbons.Select(mesh => mesh.Index).ToList();

            // ロングノーツのまとまりの個数を取得するために、その最後のインデックス (= インデックスリストの中で一番大きい値) を取得する。
            // (これは0から始まるインデックス番号なので実際はこれに+1した個数)
            int maxNumberOfLongNotes = meshIndexList.Max();

            // ribbons (meshIndexList と同等の並び) 内で重複しているノーツインデックスを抽出する。
            int[] duplicateIndexes = meshIndexList.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();

            // # 重複したものを処理

            // 新たにつくる親オブジェクトをひとまとめにするLノーツ (インデックスの重複があるLノーツ) 分だけ確保する。
            // 親オブジェクトはインデックスをその名前に入れて生成
            LongNote[] parents = duplicateIndexes.Select(i => 
            {
                return new GameObject($"LongNoteRibbon-{i}").AddComponent<LongNote>();
            }).ToArray();

            // インデックスが重複した中間点有りLノーツの欠片を入れ子構造にして、
            // インデックスごとにひとまとまりのオブジェクトができるよう調整
            for (int i = 0; i < duplicateIndexes.Length; i++)
            {
                // さっきつけた LongNote を編集
                parents[i].SetProperties(NoteType.LinearLong, duplicateIndexes[i], LongNotePart.Ribbon, true);
                
                // 親には Collider を新しく設定する
                parents[i].gameObject.AddComponent<MeshCollider>();

                for (int j = 0; j < meshIndexList.Count; j++)
                {
                    if (ribbons[j].Index == duplicateIndexes[i])
                    {
                        // インデックスが重複しているLノーツの断片を、同じインデックスをもつ親オブジェクトの子にする
                        ribbons[j].transform.SetParent(parents[i].transform);

                        // 入れ子にした断片がさらに子オブジェクトを持っているようならそれは曲線型
                        if (ribbons[j].transform.childCount > 0)
                        {
                            parents[i].SetProperties(NoteType.CurvedLong, duplicateIndexes[i], LongNotePart.Ribbon, true);
                        }
                        // 入れ子にした欠片の方はコンポーネントを削除する
                        Destroy(ribbons[j]);
                    }
                }
            }

            // # 1つしかないものを処理

            // リストの中から1つしかないインデックスと実際に格納されたリストのなかでのインデックス番号を抽出する。
            // Note：1つしかないノーツインデックス
            // OnRibbons：ribbons (meshIndexList と同等の並び) 内で、Note の値が入っている位置（ribbons でのインデックス）
            var uniqueIndexes = meshIndexList.GroupBy(x => x).Where(x => x.Count() == 1).Select(x => new { Note = x.Key, OnRibbons = meshIndexList.IndexOf(x.Key) }).ToArray();

            // 1つしかないLノーツインデックスをもつメッシュの配列たち
            // 新しいリストは順番で代入できるように一旦配列で確保
            LongNote[] newMeshes = new LongNote[maxNumberOfLongNotes + 1];

            // 新しいリスト（配列）に、Lノーツインデックス順にメッシュを追加。
            // duplicateIndexes と uniqueIndexes は互いに重複しない実際のLノーツインデックスなので、直接キーとして代入できる
            for (int i = 0; i < duplicateIndexes.Length; i++)
            {
                newMeshes[duplicateIndexes[i]] = parents[i].GetComponent<LongNote>();
            }

            for (int i = 0; i < uniqueIndexes.Length; i++)
            {
                newMeshes[uniqueIndexes[i].Note] = ribbons[uniqueIndexes[i].OnRibbons];
            }

            // 新しいリストへ変更を反映する
            ribbons.Clear();
            ribbons = newMeshes.ToList();

            // 共通の親にまとめる
            // FIXME: ribbon が ぬるりになるときがあるよう
            ribbons.ForEach(ribbon => ribbon.transform.SetParent(instanceParent));
        }

        #endregion

        #region 専用ユーティリティメソッド

        /// <summary>
        /// 曲線型ロングノーツのメッシュ座標をベジェ曲線として計算する。
        /// </summary>
        /// <remarks>
        /// https://iconcreator.hatenablog.com/entry/2021/09/13/190000
        /// </remarks>
        /// <param name = "start">ロングノーツの始点。</param>
        /// <param name = "anchor">ロングノーツを曲げる制御点。</param>
        /// <param name = "end">ロングノーツの終点。</param>
        /// <param name = "splitSize">曲線生成時の分割数。値が多い程滑らかな曲線になります。</param>
        /// <returns>
        /// 曲線をつくる点の座標。
        /// </returns>
        private Vector3[] CalculateBezierCurves(Vector3 start, Vector3 anchor, Vector3 end, int splitSize = 10)
        {
            // ベジェ曲線の点を分割するときに値を変化させる比
            float t = 0;

            // 曲線上の点の座標を格納する配列
            Vector3[] curves = new Vector3[splitSize + 1];

            // 曲線上の点を求める
            for (int i = 0; i <= splitSize; i++)
            {
                // 現在の分割回数と、最終的な分割数を割ったものが比であるtの値
                t = (float)i / splitSize;

                // 分割した点a, bを線形補間によって繰り返し計算する
                Vector3 a = Vector3.Lerp(start, anchor, t);
                Vector3 b = Vector3.Lerp(anchor, end, t);

                // 2つの分割点をもとにさらに分割する
                curves[i] = Vector3.Lerp(a, b, t);
            }

            return curves;
        }

        /// <summary>
        /// 曲線型ロングノーツを構成するメッシュの断片を一つのメッシュとして合成し、親オブジェクト（ロングノーツ１まとまり）のメッシュとして置き換える。
        /// </summary>
        /// <remarks>
        /// コライダーは合成しない（できない）
        /// </remarks>
        /// <param name = "parent">
        /// メッシュを合成するときに使う親オブジェクト（ロングノーツ１まとまりに相当）
        /// </param>
        private void CombineFragmentMesh(GameObject parent)
        {
            // 断片のメッシュを全取得
            MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>()
                .Where(fragment => fragment.gameObject != parent)
                .ToArray();

            // メッシュを合成する
            CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length];
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combineInstances[i].mesh = meshFilters[i].sharedMesh;
                combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            // 合成したメッシュを生成する
            parent.GetComponent<MeshFilter>().mesh.CombineMeshes(combineInstances);

            // 結合後の子オブジェクト（断片）の処理
            // マテリアルで透明にしておくにとどめる
            // (Destroyしたらコライダーが結合できていないままなので当たり判定が消えてしまう)
            foreach (var fragment in meshFilters)
            {
                fragment.gameObject.GetComponent<MeshRenderer>().material = ribbonMaterials.fadeMaterial;
            }
        }

        /// <summary>
        /// 結合した後のロングノーツのメッシュのUV座標を再計算してテクスチャを貼り直す。
        /// </summary>
        /// <param name = "combined">結合させたメッシュを適用したオブジェクト。</param>
        /// <param name = "split">分割数。</param>
        private void ReprintTexture(GameObject combined, float split, LongNoteType type)
        {
            // 結合後のメッシュの頂点数を取得する
            int verticesLength = combined.GetComponent<MeshFilter>().mesh.vertices.Length;

            // UVを再計算する
            Vector2[] uvs = new Vector2[verticesLength];
            for (int i = 0; i < uvs.Length; i++)
            {
                /* 以下：メッシュの上底（Y面）のUVを設定する。 */
                // 四角形左下の絞り込み。
                if (i == 0 || (i % 4 == 0 && i % 24 == 0))
                {
                    uvs[i] = new Vector2(0, Mathf.Abs(i / 24 / (float)split));
                    continue;
                }
                // 四角形右下の絞り込み。
                else if (i == 1 || ((i - 1) % 4 == 0 && (i - 1) % 24 == 0))
                {
                    uvs[i] = new Vector2(1, Mathf.Abs((i - 1) / 24 / (float)split));
                    continue;
                }
                // 四角形左上の絞り込み。
                else if (i == 2 || ((i - 2) % 4 == 0 && (i - 2) % 24 == 0))
                {
                    uvs[i] = new Vector2(0, Mathf.Abs(i / 24 / (float)split + 1f / (float)split));
                    continue;
                }
                // 四角形右上の絞り込み。
                else if (i == 3 || ((i - 3) % 4 == 0 && (i - 3) % 24 == 0))
                {
                    uvs[i] = new Vector2(1, Mathf.Abs((i - 1) / 24 / (float)split + 1f / (float)split));
                    continue;
                }
                /* 以下：メッシュの上底以外のUVを設定する。見ようにも見えない部分なので適当。 */
                else if (i % 4 == 0)
                {
                    uvs[i] = new Vector2(0, 0);
                    continue;
                }
                else if ((i - 1) % 4 == 0)
                {
                    uvs[i] = new Vector2(1, 0);
                    continue;
                }
                else if ((i - 2) % 4 == 0)
                {
                    uvs[i] = new Vector2(0, 1);
                    continue;
                }
                else if ((i - 3) % 4 == 0)
                {
                    uvs[i] = new Vector2(1, 1);
                    continue;
                }
            }

            // UVを適用
            combined.GetComponent<MeshFilter>().mesh.uv = uvs;

            // テクスチャを再貼付
            combined.GetComponent<MeshRenderer>().material = type switch
            {
                LongNoteType.DirectCurved => ribbonMaterials.direct,
                LongNoteType.IntermediateCurved => ribbonMaterials.intermediate,
                _ => combined.GetComponent<MeshRenderer>().material
            };
        }

        #endregion
    }
}

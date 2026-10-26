using System;
using System.Collections.Generic;
using Colossal;
using Game.SceneFlow;

namespace MeasureItCS2
{
    public sealed class LocaleSource : IDictionarySource
    {
        private static readonly string[] s_LocaleIds =
        {
            "en-US", "fr-FR", "de-DE", "es-ES", "it-IT", "ja-JP", "ko-KR", "pl-PL", "ru-RU",
            "zh-Hans-CN", "zh-Hant-TW", "pt-BR",
        };

        private static readonly string[] s_ExtraLocaleIds =
        {
            "en-GB", "zh-Hans", "zh-CN", "zh-Hant", "zh-TW",
        };

        private static readonly Dictionary<string, string[]> s_Text = new Dictionary<string, string[]>
        {
            { "Settings.GroupMeasurement", new[] { "Measurement", "Mesures", "Messung", "Medición", "Misurazione", "計測", "측정", "Pomiar", "Измерение", "测量", "測量", "Medição" } },
            { "Settings.GroupPanel", new[] { "Panel", "Panneau", "Fenster", "Panel", "Pannello", "パネル", "패널", "Panel", "Панель", "面板", "面板", "Painel" } },
            { "Settings.GroupKeybinds", new[] { "Keybinds", "Raccourcis", "Tastenbelegung", "Atajos", "Tasti", "キー割り当て", "단축키", "Skróty", "Клавиши", "快捷键", "快捷鍵", "Atalhos" } },
            { "Settings.GroupInformation", new[] { "Information", "Informations", "Informationen", "Información", "Informazioni", "情報", "정보", "Informacje", "Информация", "信息", "資訊", "Informações" } },
            { "Settings.Version", new[] { "Version", "Version", "Version", "Versión", "Versione", "バージョン", "버전", "Wersja", "Версия", "版本", "版本", "Versão" } },
            { "Settings.VersionDesc", new[] { "Installed version of Measure It!.", "Version installée de Measure It!.", "Installierte Version von Measure It!.", "Versión instalada de Measure It!.", "Versione installata di Measure It!.", "Measure It! のインストール済みバージョンです。", "설치된 Measure It! 버전입니다.", "Zainstalowana wersja Measure It!.", "Установленная версия Measure It!.", "Measure It! 的已安装版本。", "Measure It! 的已安裝版本。", "Versão instalada do Measure It!." } },
            { "Settings.DistanceDesc", new[] { "Choose the unit used to display measured distances.", "Choisissez l’unité d’affichage des distances mesurées.", "Wähle die Einheit für gemessene Entfernungen.", "Elige la unidad para mostrar las distancias medidas.", "Scegli l'unità per visualizzare le distanze misurate.", "計測距離の表示単位を選択します。", "측정 거리를 표시할 단위를 선택합니다.", "Wybierz jednostkę wyświetlania zmierzonych odległości.", "Выберите единицы отображения измеренных расстояний.", "选择测量距离的显示单位。", "選擇測量距離的顯示單位。", "Escolha a unidade usada para exibir as distâncias medidas." } },
            { "Settings.HeightDesc", new[] { "Choose the unit used to display point heights and elevation changes.", "Choisissez l’unité d’affichage des hauteurs et des dénivelés.", "Wähle die Einheit für Punkthöhen und Höhenunterschiede.", "Elige la unidad para mostrar alturas y cambios de elevación.", "Scegli l'unità per visualizzare le altezze dei punti e i dislivelli.", "ポイントの高さと標高差の表示単位を選択します。", "점 높이와 고도 변화를 표시할 단위를 선택합니다.", "Wybierz jednostkę wyświetlania wysokości punktów i różnic wysokości.", "Выберите единицы отображения высоты точек и перепадов высот.", "选择点高度和高差的显示单位。", "選擇點高度與高差的顯示單位。", "Escolha a unidade usada para exibir as alturas dos pontos e os desníveis." } },
            { "Settings.SlopeUnitDesc", new[] { "Choose whether slope is shown in degrees or as a percentage.", "Choisissez d’afficher la pente en degrés ou en pourcentage.", "Wähle, ob die Neigung in Grad oder Prozent angezeigt wird.", "Elige si la pendiente se muestra en grados o en porcentaje.", "La pendenza può essere visualizzata in gradi o in percentuale.", "傾斜を度数またはパーセントで表示します。", "경사를 도 또는 백분율로 표시할지 선택합니다.", "Wybierz wyświetlanie nachylenia w stopniach lub procentach.", "Выберите отображение уклона в градусах или процентах.", "选择以角度或百分比显示坡度。", "選擇以角度或百分比顯示坡度。", "Escolha se a inclinação será exibida em graus ou porcentagem." } },
            { "Settings.DirectionUnitDesc", new[] { "Choose whether direction is shown as degrees or cardinal directions.", "Choisissez d’afficher la direction en degrés ou en points cardinaux.", "Wähle, ob die Richtung in Grad oder als Himmelsrichtung angezeigt wird.", "Elige si la dirección se muestra en grados o como punto cardinal.", "Scegli se visualizzare la direzione in gradi o come punto cardinale.", "方向を度数または方位で表示します。", "방향을 도 또는 방위로 표시할지 선택합니다.", "Wybierz wyświetlanie kierunku w stopniach lub jako kierunku świata.", "Выберите отображение направления в градусах или по сторонам света.", "选择以角度或方位显示方向。", "選擇以角度或方位顯示方向。", "Escolha se a direção será exibida em graus ou como ponto cardeal." } },
            { "Settings.AreaDesc", new[] { "Choose the unit used to display measured areas.", "Choisissez l’unité d’affichage des surfaces mesurées.", "Wähle die Einheit für gemessene Flächen.", "Elige la unidad para mostrar las superficies medidas.", "Scegli l'unità per visualizzare le aree misurate.", "計測面積の表示単位を選択します。", "측정 면적을 표시할 단위를 선택합니다.", "Wybierz jednostkę wyświetlania zmierzonych powierzchni.", "Выберите единицы отображения измеренных площадей.", "选择测量面积的显示单位。", "選擇測量面積的顯示單位。", "Escolha a unidade usada para exibir as áreas medidas." } },
            { "Settings.ResetPanel", new[] { "Reset panel position", "Réinitialiser la position du panneau", "Panelposition zurücksetzen", "Restablecer posición del panel", "Reimposta posizione pannello", "パネル位置をリセット", "패널 위치 초기화", "Resetuj pozycję panelu", "Сбросить положение панели", "重置面板位置", "重設面板位置", "Redefinir posição do painel" } },
            { "Settings.MeasureColor", new[] { "Line color", "Couleur des lignes", "Linienfarbe", "Color de línea", "Colore delle linee", "線の色", "선 색상", "Kolor linii", "Цвет линий", "线条颜色", "線條顏色", "Cor das linhas" } },
            { "Settings.SnapNodes", new[] { "Snap to nodes", "Accrocher aux nœuds", "An Knoten einrasten", "Ajustar a nodos", "Aggancia ai nodi", "ノードにスナップ", "노드에 스냅", "Przyciągaj do węzłów", "Привязка к узлам", "吸附到节点", "吸附至節點", "Ajustar aos nós" } },
            { "Settings.ForceGround", new[] { "Force height to ground", "Forcer la hauteur au sol", "Höhe auf Bodenhöhe setzen", "Forzar altura al terreno", "Forza altezza al terreno", "地面の高さに固定", "지면 높이로 고정", "Wymuś wysokość terenu", "Высота по поверхности земли", "强制使用地面高度", "強制使用地面高度", "Forçar altura ao solo" } },
            { "Settings.LockHeight", new[] { "Lock current height", "Verrouiller la hauteur actuelle", "Aktuelle Höhe fixieren", "Bloquear altura actual", "Blocca altezza corrente", "現在の高さを固定", "현재 높이 고정", "Zablokuj bieżącą wysokość", "Зафиксировать текущую высоту", "锁定当前高度", "鎖定目前高度", "Travar altura atual" } },
            { "Settings.ToggleTool", new[] { "Toggle Measure It!", "Activer/désactiver Measure It!", "Measure It! umschalten", "Activar/desactivar Measure It!", "Attiva/disattiva Measure It!", "Measure It! の切り替え", "Measure It! 켜기/끄기", "Przełącz Measure It!", "Включить/выключить Measure It!", "切换 Measure It!", "切換 Measure It!", "Alternar Measure It!" } },
            { "Settings.AddPoint", new[] { "Add point", "Ajouter un point", "Punkt hinzufügen", "Añadir punto", "Aggiungi punto", "ポイントを追加", "점 추가", "Dodaj punkt", "Добавить точку", "添加点", "新增點", "Adicionar ponto" } },
            { "Settings.Undo", new[] { "Undo last point", "Annuler le dernier point", "Letzten Punkt rückgängig", "Deshacer último punto", "Annulla ultimo punto", "最後のポイントを元に戻す", "마지막 점 실행 취소", "Cofnij ostatni punkt", "Отменить последнюю точку", "撤销上一个点", "復原上一個點", "Desfazer último ponto" } },
            { "Settings.ToggleToolDesc", new[] { "Keyboard shortcut to activate or deactivate Measure It!.", "Raccourci pour activer ou désactiver Measure It!.", "Tastenkürzel zum Aktivieren oder Deaktivieren von Measure It!.", "Atajo para activar o desactivar Measure It!.", "Scorciatoia per attivare o disattivare Measure It!.", "Measure It! を有効または無効にするショートカットです。", "Measure It!을 켜거나 끄는 단축키입니다.", "Skrót do włączania lub wyłączania Measure It!.", "Клавиша для включения и выключения Measure It!.", "用于启用或停用 Measure It! 的快捷键。", "用於啟用或停用 Measure It! 的快捷鍵。", "Atalho para ativar ou desativar o Measure It!." } },
            { "Settings.AddPointDesc", new[] { "Keyboard shortcut to add a measurement point at the cursor.", "Raccourci pour ajouter un point de mesure à la position du curseur.", "Tastenkürzel zum Setzen eines Messpunkts an der Cursorposition.", "Atajo para añadir un punto de medición en la posición del cursor.", "Scorciatoia per aggiungere un punto di misurazione alla posizione del cursore.", "カーソル位置に計測ポイントを追加するショートカットです。", "커서 위치에 측정 지점을 추가하는 단축키입니다.", "Skrót do dodawania punktu pomiarowego w miejscu kursora.", "Клавиша для добавления точки измерения в позиции курсора.", "用于在光标位置添加测量点的快捷键。", "用於在游標位置新增測量點的快捷鍵。", "Atalho para adicionar um ponto de medição na posição do cursor." } },
            { "Settings.UndoDesc", new[] { "Keyboard shortcut to remove the last measurement point.", "Raccourci pour supprimer le dernier point de mesure.", "Tastenkürzel zum Entfernen des letzten Messpunkts.", "Atajo para eliminar el último punto de medición.", "Scorciatoia per rimuovere l'ultimo punto di misurazione.", "最後に追加した計測ポイントを削除するショートカットです。", "마지막 측정 지점을 제거하는 단축키입니다.", "Skrót do usuwania ostatniego punktu pomiarowego.", "Клавиша для удаления последней точки измерения.", "用于删除上一个测量点的快捷键。", "用於刪除上一個測量點的快捷鍵。", "Atalho para remover o último ponto de medição." } },
            { "Settings.ResetPanelDesc", new[] { "Return the panel to its default position.", "Replace le panneau à sa position par défaut.", "Setzt das Fenster auf die Standardposition zurück.", "Devuelve el panel a su posición predeterminada.", "Riporta il pannello nella posizione predefinita.", "パネルを既定の位置に戻します。", "패널을 기본 위치로 되돌립니다.", "Przywraca domyślne położenie panelu.", "Возвращает панель в исходное положение.", "将面板恢复到默认位置。", "將面板恢復至預設位置。", "Restaura a posição padrão do painel." } },
            { "Settings.MeasureColorDesc", new[] { "Color of points and measurement lines.", "Couleur des points et des lignes de mesure.", "Farbe der Punkte und Messlinien.", "Color de los puntos y las líneas de medición.", "Colore dei punti e delle linee di misurazione.", "ポイントと計測線の色です。", "측정 점과 선의 색상입니다.", "Kolor punktów i linii pomiarowych.", "Цвет точек и измерительных линий.", "测量点和线条的颜色。", "測量點與線條的顏色。", "Cor dos pontos e das linhas de medição." } },
            { "Settings.SnapDesc", new[] { "Snap placed points to nearby network nodes.", "Accroche les points posés aux nœuds de réseau proches.", "Rastet gesetzte Punkte an nahegelegenen Netzwerkknoten ein.", "Ajusta los puntos colocados a nodos de red cercanos.", "Aggancia i punti ai nodi di rete vicini.", "配置したポイントを近くのネットワークノードにスナップします。", "배치한 점을 주변 네트워크 노드에 스냅합니다.", "Przyciąga punkty do pobliskich węzłów sieci.", "Привязывает точки к ближайшим узлам сети.", "将测量点吸附到附近的网络节点。", "將測量點吸附至附近的網路節點。", "Ajusta os pontos aos nós de rede próximos." } },
            { "Settings.GroundDesc", new[] { "Place points at the terrain height under the cursor.", "Place les points à la hauteur du terrain sous le curseur.", "Setzt Punkte auf die Geländehöhe unter dem Cursor.", "Coloca los puntos a la altura del terreno bajo el cursor.", "Posiziona i punti all'altezza del terreno sotto il cursore.", "カーソル下の地形の高さにポイントを配置します。", "커서 아래 지형 높이에 점을 배치합니다.", "Umieszcza punkty na wysokości terenu pod kursorem.", "Размещает точки на высоте поверхности под курсором.", "将点放置在光标下方的地形高度。", "將點放置在游標下方的地形高度。", "Coloca os pontos na altura do terreno sob o cursor." } },
            { "Settings.LockDesc", new[] { "Keep new points at the last point's height. The first point stays free when none exists.", "Place les nouveaux points à la hauteur du dernier. Le premier reste libre s'il n'y en a pas encore.", "Hält neue Punkte auf der Höhe des letzten Punkts. Ohne Punkt bleibt der erste frei.", "Mantiene los puntos nuevos a la altura del último. Si no hay ninguno, el primero queda libre.", "Mantiene i nuovi punti all'altezza dell'ultimo. Se non esiste, il primo resta libero.", "以最後のポイントの高さに固定します。ポイントがない場合、最初のポイントは自由です。", "새 점을 마지막 점의 높이에 맞춥니다. 점이 없으면 첫 점은 자유롭게 둡니다.", "Utrzymuje nowe punkty na wysokości ostatniego. Pierwszy pozostaje swobodny, jeśli go brak.", "Фиксирует новые точки на высоте последней. Если точек нет, первая ставится свободно.", "将新点保持在上一个点的高度。尚无点时，第一个点不受限制。", "將新點保持在上一個點的高度。尚無點時，第一個點不受限制。", "Mantém os novos pontos na altura do último. Sem pontos, o primeiro fica livre." } },

            { "UI.UnitsSetup", new[] { "Units setup", "Unités", "Einheiten", "Unidades", "Unità", "単位", "단위", "Jednostki", "Единицы", "单位", "單位", "Unidades" } },
            { "UI.Distance", new[] { "Distance", "Distance", "Entfernung", "Distancia", "Distanza", "距離", "거리", "Dystans", "Расстояние", "距离", "距離", "Distância" } },
            { "UI.Height", new[] { "Height", "Hauteur", "Höhe", "Altura", "Altezza", "高さ", "높이", "Wysokość", "Высота", "高度", "高度", "Altura" } },
            { "UI.Slope", new[] { "Slope", "Pente", "Neigung", "Pendiente", "Pendenza", "傾斜", "경사", "Nachylenie", "Уклон", "坡度", "坡度", "Inclinação" } },
            { "UI.Direction", new[] { "Direction", "Direction", "Richtung", "Dirección", "Direzione", "方角", "방향", "Kierunek", "Направление", "方向", "方向", "Direção" } },
            { "UI.Area", new[] { "Area", "Surface", "Fläche", "Área", "Area", "面積", "면적", "Powierzchnia", "Площадь", "面积", "面積", "Área" } },
            { "UI.SnapShort", new[] { "Snap", "Nœuds", "Knoten", "Nodos", "Nodi", "ノード", "노드", "Węzły", "Узлы", "节点", "節點", "Nós" } },
            { "UI.GroundShort", new[] { "Ground", "Sol", "Boden", "Suelo", "Suolo", "地面", "지면", "Teren", "Земля", "地面", "地面", "Solo" } },
            { "UI.LockShort", new[] { "Lock", "Verrou", "Sperren", "Bloq.", "Blocca", "固定", "고정", "Blokuj", "Фикс.", "锁定", "鎖定", "Travar" } },
            { "UI.SnapTooltip", new[] { "Snap points to nearby network nodes", "Accrocher les points aux nœuds de réseau proches", "Punkte an nahegelegenen Netzwerkknoten einrasten", "Ajustar puntos a nodos de red cercanos", "Aggancia i punti ai nodi di rete vicini", "近くのネットワークノードにスナップ", "주변 네트워크 노드에 스냅", "Przyciągaj punkty do pobliskich węzłów", "Привязывать точки к ближайшим узлам", "吸附到附近的网络节点", "吸附至附近的網路節點", "Ajustar aos nós de rede próximos" } },
            { "UI.GroundTooltip", new[] { "Force points to terrain height", "Forcer les points à la hauteur du terrain", "Punkte auf Geländehöhe setzen", "Forzar puntos a la altura del terreno", "Forza i punti all'altezza del terreno", "地形の高さに固定", "지형 높이로 고정", "Wymuś wysokość terenu", "Фиксировать точки на высоте земли", "强制使用地面高度", "強制使用地面高度", "Forçar altura do terreno" } },
            { "UI.LockTooltip", new[] { "Keep new points at the last point's height; the first point stays free", "Garder les nouveaux points à la hauteur du dernier ; le premier reste libre", "Neue Punkte auf der Höhe des letzten halten; der erste bleibt frei", "Mantener los puntos nuevos a la altura del último; el primero queda libre", "Mantieni i nuovi punti all'altezza dell'ultimo; il primo resta libero", "最後のポイントの高さを維持。最初のポイントは自由", "마지막 점의 높이를 유지합니다. 첫 점은 자유롭게 둡니다", "Utrzymuj wysokość ostatniego punktu; pierwszy pozostaje swobodny", "Сохранять высоту последней точки; первая ставится свободно", "保持上一个点的高度；第一个点不受限制", "保持上一個點的高度；第一個點不受限制", "Manter a altura do último ponto; o primeiro fica livre" } },
            { "UI.AreaPreview", new[] { "Area preview", "Aperçu de surface", "Flächenvorschau", "Vista previa del área", "Anteprima area", "面積プレビュー", "면적 미리보기", "Podgląd powierzchni", "Предпросмотр площади", "面积预览", "面積預覽", "Prévia da área" } },
            { "UI.NetElevation", new[] { "Net elevation", "Dénivelé net", "Höhendifferenz", "Desnivel neto", "Dislivello netto", "標高差", "순고도", "Różnica wysokości", "Перепад высот", "净高差", "淨高差", "Desnível líquido" } },
            { "UI.TotalAscent", new[] { "Total ascent", "Dénivelé positif", "Gesamtanstieg", "Ascenso total", "Salita totale", "累積上昇", "총 상승", "Łączne podejście", "Общий подъём", "总爬升", "總爬升", "Subida total" } },
            { "UI.PlanDistance", new[] { "Plan distance", "Distance au sol", "Horizontale Entfernung", "Distancia horizontal", "Distanza in pianta", "水平距離", "수평 거리", "Dystans poziomy", "Расстояние по горизонтали", "水平距离", "水平距離", "Distância horizontal" } },
            { "UI.TotalDescent", new[] { "Total descent", "Dénivelé négatif", "Gesamtabstieg", "Descenso total", "Discesa totale", "累積下降", "총 하강", "Łączne zejście", "Общий спуск", "总下降", "總下降", "Descida total" } },
            { "UI.Segments", new[] { "Segments", "Segments", "Abschnitte", "Segmentos", "Segmenti", "区間", "구간", "Odcinki", "Сегменты", "线段", "線段", "Segmentos" } },
            { "UI.SegmentDistance", new[] { "DIST.", "DIST.", "ENTF.", "DIST.", "DIST.", "距離", "거리", "DYST.", "РАССТ.", "距离", "距離", "DIST." } },
            { "UI.SegmentRise", new[] { "RISE", "DÉNIV.", "ANSTIEG", "DESNIVEL", "DISLIV.", "上昇", "상승", "WZN.", "ПОДЪЁМ", "升高", "升高", "SUBIDA" } },
            { "UI.SegmentGrade", new[] { "GRADE", "PENTE", "GEFÄLLE", "PENDIENTE", "PENDENZA", "勾配", "경사", "NACHYL.", "УКЛОН", "坡度", "坡度", "INCLINAÇÃO" } },
            { "UI.SegmentBearing", new[] { "BEARING", "AZIMUT", "RICHTUNG", "RUMBO", "DIREZIONE", "方位", "방위", "AZYMUT", "АЗИМУТ", "方位", "方位", "DIREÇÃO" } },
            { "UI.Clear", new[] { "Clear", "Effacer", "Leeren", "Borrar", "Cancella", "消去", "지우기", "Wyczyść", "Очистить", "清除", "清除", "Limpar" } },
            { "UI.CopyRaw", new[] { "Copy RAW", "Copier les données", "Rohdaten kopieren", "Copiar datos", "Copia dati grezzi", "RAWをコピー", "RAW 복사", "Kopiuj dane", "Копировать данные", "复制原始数据", "複製原始資料", "Copiar dados brutos" } },
            { "UI.Copied", new[] { "Copied", "Copié", "Kopiert", "Copiado", "Copiato", "コピーしました", "복사됨", "Skopiowano", "Скопировано", "已复制", "已複製", "Copiado" } },
            { "UI.CopyFailed", new[] { "Copy failed", "Échec de la copie", "Kopieren fehlgeschlagen", "Error al copiar", "Copia non riuscita", "コピーできませんでした", "복사 실패", "Kopiowanie nie powiodło się", "Не удалось скопировать", "复制失败", "複製失敗", "Falha ao copiar" } },
            { "UI.LineColor", new[] { "Line color", "Couleur des lignes", "Linienfarbe", "Color de línea", "Colore delle linee", "線の色", "선 색상", "Kolor linii", "Цвет линий", "线条颜色", "線條顏色", "Cor das linhas" } },
            { "UI.Tooltip", new[] { "Measure It! (Ctrl+5) — click to place measurement points", "Measure It! (Ctrl+5) — cliquez pour placer des points", "Measure It! (Strg+5) — klicken, um Messpunkte zu setzen", "Measure It! (Ctrl+5) — haz clic para colocar puntos", "Measure It! (Ctrl+5) — fai clic per posizionare i punti", "Measure It! (Ctrl+5) — クリックして計測点を配置", "Measure It! (Ctrl+5) — 클릭해 측정 지점을 배치", "Measure It! (Ctrl+5) — kliknij, aby ustawić punkty", "Measure It! (Ctrl+5) — нажмите, чтобы поставить точки", "Measure It! (Ctrl+5) — 点击放置测量点", "Measure It! (Ctrl+5) — 點擊放置測量點", "Measure It! (Ctrl+5) — clique para posicionar pontos" } },

            { "Unit.Meters", new[] { "Meters", "Mètres", "Meter", "Metros", "Metri", "メートル", "미터", "Metry", "Метры", "米", "公尺", "Metros" } },
            { "Unit.Kilometers", new[] { "Kilometers", "Kilomètres", "Kilometer", "Kilómetros", "Chilometri", "キロメートル", "킬로미터", "Kilometry", "Километры", "千米", "公里", "Quilômetros" } },
            { "Unit.Feet", new[] { "Feet", "Pieds", "Fuß", "Pies", "Piedi", "フィート", "피트", "Stopy", "Футы", "英尺", "英尺", "Pés" } },
            { "Unit.Miles", new[] { "Miles", "Miles", "Meilen", "Millas", "Miglia", "マイル", "마일", "Mile", "Мили", "英里", "英里", "Milhas" } },
            { "Unit.Degrees", new[] { "Degrees", "Degrés", "Grad", "Grados", "Gradi", "度", "도", "Stopnie", "Градусы", "度", "度", "Graus" } },
            { "Unit.Percentage", new[] { "Percentage", "Pourcentage", "Prozent", "Porcentaje", "Percentuale", "パーセント", "퍼센트", "Procent", "Проценты", "百分比", "百分比", "Porcentagem" } },
            { "Unit.Cardinal", new[] { "Cardinal", "Cardinal", "Himmelsrichtung", "Cardinal", "Cardinale", "方位", "방위", "Kierunki świata", "Румб", "方位", "方位", "Cardinal" } },
            { "Area.SquareMeters", new[] { "m²", "m²", "m²", "m²", "m²", "m²", "m²", "m²", "м²", "平方米", "平方公尺", "m²" } },
            { "Area.Hectares", new[] { "ha", "ha", "ha", "ha", "ha", "ha", "ha", "ha", "га", "公顷", "公頃", "ha" } },
            { "Area.SquareKilometers", new[] { "km²", "km²", "km²", "km²", "km²", "km²", "km²", "km²", "км²", "平方千米", "平方公里", "km²" } },
            { "Area.SquareFeet", new[] { "ft²", "ft²", "ft²", "ft²", "ft²", "ft²", "ft²", "ft²", "фут²", "平方英尺", "平方英尺", "ft²" } },
            { "Area.Acres", new[] { "Acres", "Acres", "Acres", "Acres", "Acri", "エーカー", "에이커", "Akry", "Акры", "英亩", "英畝", "Acres" } },
            { "Color.Red", new[] { "Red", "Rouge", "Rot", "Rojo", "Rosso", "赤", "빨강", "Czerwony", "Красный", "红色", "紅色", "Vermelho" } },
            { "Color.Yellow", new[] { "Yellow", "Jaune", "Gelb", "Amarillo", "Giallo", "黄", "노랑", "Żółty", "Жёлтый", "黄色", "黃色", "Amarelo" } },
            { "Color.Magenta", new[] { "Magenta", "Magenta", "Magenta", "Magenta", "Magenta", "マゼンタ", "마젠타", "Magenta", "Пурпурный", "洋红色", "洋紅色", "Magenta" } },
            { "Color.Green", new[] { "Green", "Vert", "Grün", "Verde", "Verde", "緑", "초록", "Zielony", "Зелёный", "绿色", "綠色", "Verde" } },
            { "Color.Cyan", new[] { "Cyan", "Cyan", "Cyan", "Cian", "Ciano", "シアン", "시안", "Cyjan", "Голубой", "青色", "青色", "Ciano" } },
            { "Color.White", new[] { "White", "Blanc", "Weiß", "Blanco", "Bianco", "白", "흰색", "Biały", "Белый", "白色", "白色", "Branco" } },
            { "Color.Orange", new[] { "Orange", "Orange", "Orange", "Naranja", "Arancione", "オレンジ", "주황", "Pomarańczowy", "Оранжевый", "橙色", "橙色", "Laranja" } },
        };

        private readonly Setting m_Setting;
        private readonly int m_LanguageIndex;

        public LocaleSource(Setting setting, string localeId)
        {
            m_Setting = setting;
            m_LanguageIndex = GetLanguageIndex(localeId);
        }

        public static IEnumerable<string> SupportedLocaleIds
        {
            get
            {
                foreach (string localeId in s_LocaleIds)
                {
                    yield return localeId;
                }
                foreach (string localeId in s_ExtraLocaleIds)
                {
                    yield return localeId;
                }
            }
        }

        public static string Key(string key) => $"MeasureItCS2.{key}";

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            var entries = new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Measure It!" },
                { m_Setting.GetOptionGroupLocaleID(Setting.GroupTool), Text("Settings.GroupMeasurement") },
                { m_Setting.GetOptionGroupLocaleID(Setting.GroupPanel), Text("Settings.GroupPanel") },
                { m_Setting.GetOptionGroupLocaleID(Setting.GroupKeybinds), Text("Settings.GroupKeybinds") },
                { m_Setting.GetOptionGroupLocaleID(Setting.GroupInformation), Text("Settings.GroupInformation") },
            };

            AddOption(entries, nameof(Setting.UnitOfDistance), "UI.Distance", "Settings.DistanceDesc");
            AddOption(entries, nameof(Setting.UnitOfHeight), "UI.Height", "Settings.HeightDesc");
            AddOption(entries, nameof(Setting.UnitOfSlope), "UI.Slope", "Settings.SlopeUnitDesc");
            AddOption(entries, nameof(Setting.UnitOfDirection), "UI.Direction", "Settings.DirectionUnitDesc");
            AddOption(entries, nameof(Setting.UnitOfArea), "UI.Area", "Settings.AreaDesc");
            AddOption(entries, nameof(Setting.Version), "Settings.Version", "Settings.VersionDesc");
            AddOption(entries, nameof(Setting.MeasureColor), "Settings.MeasureColor", "Settings.MeasureColorDesc");
            AddOption(entries, nameof(Setting.SnapToNodes), "Settings.SnapNodes", "Settings.SnapDesc");
            AddOption(entries, nameof(Setting.ForceGroundHeight), "Settings.ForceGround", "Settings.GroundDesc");
            AddOption(entries, nameof(Setting.LockCurrentHeight), "Settings.LockHeight", "Settings.LockDesc");
            AddOption(entries, nameof(Setting.ResetPanelPosition), "Settings.ResetPanel", "Settings.ResetPanelDesc");

            AddBinding(entries, nameof(Setting.ToggleToolBinding), "Settings.ToggleTool", "Settings.ToggleToolDesc");
            AddBinding(entries, nameof(Setting.AddPointBinding), "Settings.AddPoint", "Settings.AddPointDesc");
            AddBinding(entries, nameof(Setting.UndoLastPointBinding), "Settings.Undo", "Settings.UndoDesc");

            foreach (KeyValuePair<string, string[]> row in s_Text)
            {
                entries[Key(row.Key)] = row.Value[m_LanguageIndex];
            }

            return entries;
        }

        public void Unload() { }

        private void AddOption(Dictionary<string, string> entries, string property, string labelKey, string descriptionKey = null)
        {
            entries[m_Setting.GetOptionLabelLocaleID(property)] = Text(labelKey);
            if (descriptionKey != null)
            {
                entries[m_Setting.GetOptionDescLocaleID(property)] = Text(descriptionKey);
            }
        }

        private void AddBinding(Dictionary<string, string> entries, string property, string labelKey, string descriptionKey)
        {
            string text = Text(labelKey);
            entries[m_Setting.GetOptionLabelLocaleID(property)] = text;
            entries[m_Setting.GetOptionDescLocaleID(property)] = Text(descriptionKey);
            entries[m_Setting.GetBindingKeyLocaleID(property)] = text;
        }

        private string Text(string key) => s_Text.TryGetValue(key, out string[] values) ? values[m_LanguageIndex] : key;

        public static string GetLocalizedText(string key)
        {
            string localeId = GameManager.instance?.localizationManager?.activeLocaleId ?? "en-US";
            return s_Text.TryGetValue(key, out string[] values) ? values[GetLanguageIndex(localeId)] : key;
        }

        private static int GetLanguageIndex(string localeId)
        {
            if (localeId.StartsWith("fr", StringComparison.OrdinalIgnoreCase)) return 1;
            if (localeId.StartsWith("de", StringComparison.OrdinalIgnoreCase)) return 2;
            if (localeId.StartsWith("es", StringComparison.OrdinalIgnoreCase)) return 3;
            if (localeId.StartsWith("it", StringComparison.OrdinalIgnoreCase)) return 4;
            if (localeId.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) return 5;
            if (localeId.StartsWith("ko", StringComparison.OrdinalIgnoreCase)) return 6;
            if (localeId.StartsWith("pl", StringComparison.OrdinalIgnoreCase)) return 7;
            if (localeId.StartsWith("ru", StringComparison.OrdinalIgnoreCase)) return 8;
            if (localeId.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase) || localeId.Equals("zh-TW", StringComparison.OrdinalIgnoreCase)) return 10;
            if (localeId.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) return 9;
            if (localeId.StartsWith("pt", StringComparison.OrdinalIgnoreCase)) return 11;
            return 0;
        }
    }
}

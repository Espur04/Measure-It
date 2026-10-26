import { bindValue, useValue } from "cs2/api";
import styles from "./MeasureItLabels.module.scss";

const GROUP = "measureItCS2";

const toolActive$ = bindValue<boolean>(GROUP, "toolActive", false);
const pointLabelsJson$ = bindValue<string>(GROUP, "pointLabelsJson", "[]");
const cursorJson$ = bindValue<string>(GROUP, "cursorJson", "{\"visible\":false,\"x\":0,\"y\":0}");
const pointColorHex$ = bindValue<string>(GROUP, "pointColorHex", "#FF2626");

interface PointLabel {
    index: number;
    x: number;
    y: number;
    visible: boolean;
}

interface CursorMarker {
    x: number;
    y: number;
    visible: boolean;
}


function safeParse<T>(json: string, fallback: T): T {
    try {
        return JSON.parse(json) as T;
    } catch {
        return fallback;
    }
}


export function MeasureItLabels() {
    const toolActive = useValue(toolActive$);
    const pointLabelsJson = useValue(pointLabelsJson$);
    const cursorJson = useValue(cursorJson$);
    const pointColorHex = useValue(pointColorHex$);

    if (!toolActive) {
        return null;
    }

    const labels: PointLabel[] = safeParse(pointLabelsJson, []);
    const cursor: CursorMarker = safeParse(cursorJson, { x: 0, y: 0, visible: false });

    return (
        <div className={styles.overlay}>
            {cursor.visible && (
                <div
                    className={styles.cursor}
                    style={{ left: `${cursor.x}px`, top: `${cursor.y}px` }}
                    aria-hidden="true"
                >
                    <span className={styles.cursorHorizontal} />
                    <span className={styles.cursorVertical} />
                    <span className={styles.cursorCenter} />
                </div>
            )}
            {labels
                .filter((label) => label.visible && label.index > 0)
                .map((label) => (
                    <div
                        key={label.index}
                        className={styles.label}
                        style={{
                            left: `${label.x}px`,
                            top: `${label.y}px`,
                            color: pointColorHex,
                            borderColor: pointColorHex,
                        }}
                    >
                        {label.index}
                    </div>
                ))}
        </div>
    );
}

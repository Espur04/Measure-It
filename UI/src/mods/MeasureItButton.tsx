import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { Button } from "cs2/ui";
import measureIcon from "./measureIcon.svg";
import styles from "./MeasureItButton.module.scss";

const GROUP = "measureItCS2";
const toolActive$ = bindValue<boolean>(GROUP, "toolActive", false);


export function MeasureItButton() {
    const active = useValue(toolActive$);
    const localization = useLocalization();
    const tooltip = localization.translate("MeasureItCS2.UI.Tooltip", "Measure It! (Ctrl+5) — click to place measurement points") ?? "Measure It! (Ctrl+5) — click to place measurement points";

    return (
        <Button
            src={measureIcon}
            variant="floating"
            selected={active}
            className={`${styles.toggle} ${active ? styles.selected : ""}`}
            tooltipLabel={tooltip}
            onSelect={() => trigger(GROUP, "toggleTool")}
        />
    );
}

import { ModRegistrar } from "cs2/modding";
import { MeasureItPanel } from "mods/MeasureItPanel";
import { MeasureItButton } from "mods/MeasureItButton";
import { MeasureItLabels } from "mods/MeasureItLabels";


const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.append('UniversalModMenu', MeasureItButton);

    moduleRegistry.append('Game', MeasureItPanel);

    moduleRegistry.append('Game', MeasureItLabels);

    moduleRegistry.append('Editor', MeasureItPanel);
    moduleRegistry.append('Editor', MeasureItLabels);
};

export default register;

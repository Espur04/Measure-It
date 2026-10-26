declare module "cohtml/cohtml" {
  interface SingleArgumentCallback<T> {
    (result: T): any;
  }

  interface ArbitraryCallback {
    (...args: any[]): any; // modified by Colossal
  }

  export interface EventHandle { // modified by Colossal

    clear(): void;
  }

  interface VirtualList {

    startIndex: number;


    pageSize: number;
  }

  interface AttributeHandler {

    init(element: Element, value: any): void;


    deinit(element: Element): void;


    update(element: Element, value: any): void;
  }

  interface AttributeHandlerConstructor {
    new(): AttributeHandler;
  }

  interface Deferred<T> extends Promise<T> {

    resolve(value: T): void;


    reject(value: T): void;
  }

  export interface Engine { // modified by Colossal

    whenReady: Promise<any>;


    on(name: string, callback: ArbitraryCallback, context?: any): EventHandle;


    off(name: string, callback?: ArbitraryCallback, context?: any): void;


    trigger(name: string, ...args: any[]): void;


    call(name: string, ...args: any[]): Promise<any>;


    createJSModel(name: string, model: object): void;


    synchronizeModels(): void;


    updateWholeModel(model: object): void;


    unregisterModel(model: object): void;


    createVirtualList(): VirtualList;


    registerBindingAttribute(attributeName: string, attributeHandler: AttributeHandlerConstructor): void;


    enableSafeDataBinding(enable: boolean): void;


    pinUnsafeReference(property: object): object;


    translate(id: string): string;


    reloadLocalization(): void;


    enableImmediateLayout(isEnabled: boolean): void;


    isImmediateLayoutEnabled(): boolean;


    executeImmediateLayoutSync(): void;
  }

  export const engine: Engine; // modified by Colossal
  export default engine;

}
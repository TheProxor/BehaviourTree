# Behaviour Tree

Переносимое ядро BehaviourTree и визуальный редактор для **Unity 6 (6000.0+)**.
Runtime зависит только от Unity. Editor использует `com.unity.nuget.newtonsoft-json` 3.2.1.

## Установка в другой проект

Выберите один способ:

1. **UPM:** скопируйте всю папку `com.swat.behaviour-tree` за пределы `Assets` нового проекта. В Package Manager выберите **Install package from disk…** и укажите её `package.json`. Также можно разместить папку непосредственно в `Packages` как embedded package.
2. **Assets:** скопируйте папку вместе со всеми `.meta` в `Assets` нового проекта. Установите `com.unity.nuget.newtonsoft-json` через Package Manager, если его ещё нет. При копировании в Assets зависимости из `package.json` автоматически не устанавливаются.

Не подключайте одну копию через UPM и вторую через Assets одновременно.
Если ваш код находится в собственной `.asmdef`, добавьте ссылку на `Swat.BehaviourTree`; для расширений редактора также добавьте `Swat.BehaviourTree.Editor` и ограничьте сборку платформой Editor.

## Первый запуск

1. Выберите **BehaviourTree → Create Example Tree**, сохраните ассет в `Assets`.
2. Добавьте **Behaviour Tree Runner** на GameObject и назначьте созданный ассет в поле Tree Asset.
3. Запустите Play Mode: дерево выведет `Hello from Behaviour Tree` в Console.
4. Откройте ассет двойным щелчком или через **BehaviourTree → Open**. Добавляйте узлы через контекстное меню графа.

Runner создаёт собственную копию ассета, привязывает контекст и обновляет её каждый кадр. Исходный ассет не меняется. Поле Loop включает повторный запуск завершённого дерева. Для собственного управления используйте `Clone()`, `Bind(context)`, `UpdateTree()` и `Dispose()`, после чего уничтожьте созданную копию.

## Расширение

Существующие пространства имён `Swat.Game.BehaviourTree` и `Editor.BehaviourTreeEditor` сохранены для совместимости. Они не означают зависимость от игры Swat.

```csharp
using System;
using Swat.Game.BehaviourTree;

[Serializable]
public class MyAction : ActionNode
{
    protected override NodeState OnUpdate()
    {
        // Доступны Context.Transform и Blackboard.
        return NodeState.Success;
    }
}
```

Для игровых сервисов наследуйте `RuntimeTreeContext`, затем передайте его в `Bind`. В наследнике `BehaviourTreeRunner` можно переопределить `CreateContext()`. В игровых узлах приводите `Context` к своему типу. Узлы обнаруживаются редактором через Unity TypeCache, включая пользовательские сборки.

Редактор допускает игровые расширения: `GameSpecifics.CycledTreeCondition`, `GameSpecifics.BlackboardDefaultBoolKeysNames` и `NodeInfoDrawer.Register<T>()`. Регистрируйте их из Editor-сборки проекта через `[InitializeOnLoad]`.

Пользовательские настройки сохраняются в Assets. JSON-шаблоны сохраняются в `Assets/BehaviourTree/Templates`, а не в установленный пакет.

## Что входит

- Runtime: деревья-ассеты и MonoBehaviour, runner, blackboard, события и вложенные деревья, базовые узлы, Selector, Sequencer, Parallel, RandomSelector, Wait, Cooldown, Chance, Loop и простые действия.
- Editor: граф, инспекторы, редактор blackboard, JSON-шаблоны и шаблоны C#.
- Игровые действия Swat (оружие, волны, AI, UI, игровые сервисы), их drawers, настройки и игровые JSON-шаблоны остаются в исходном проекте и не входят в пакет.

## Совместимость с существующим Swat

Сохранены GUID перенесённых файлов и имена типов. Для сериализуемых runtime-типов добавлен `MovedFrom` со старой сборкой `Game`; JSON-конвертер поддерживает старые имена сборок. `Game` теперь использует ядро `Swat.BehaviourTree`, а игровые расширения редактора используют `Swat.BehaviourTree.Editor`.

Семантика узлов сохранена: Sequencer по умолчанию продолжает после Failure, Parallel в режиме Immediate ориентируется на первого ребёнка, Loop считает успешные завершения. Это отличается от некоторых других реализаций Behaviour Tree.

Подробное описание: [базовые узлы](Documentation/Nodes.md).

## Проверка переноса

Проверено в Unity 6000.0.60f1: установка UPM в отдельный чистый проект без Game/Zenject, открытие графа, загрузка шаблонов C#, выполнение дерева, независимость клона, загрузка JSON со старым именем сборки. В исходном Swat прочитаны 22 игровых JSON-шаблона и проверены 6428 компонентов деревьев в префабах — отсутствующих managed-reference типов не обнаружено. Полная игровая сессия и сборка Player в эту проверку не входят.

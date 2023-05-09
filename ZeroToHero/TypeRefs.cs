using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResource {
}
public interface IValuedResource<T> : IResource {
}
public interface IIntResource : IValuedResource<SInt> {
}
public class TemporaryIntResource : IIntResource {
}
public interface IBoolResource : IValuedResource<SBool> { }
public class TemporaryBoolResource : IBoolResource {
}
public class TemporaryPositionResource : IValuedResource<SInt2> {
}
public class SymbolResource<T> : IValuedResource<T> where T : ISerializable {
}

[TypeIndex(216)]
public class SetStatAction : SObj, IAction {
    public SInt NewValue { get => Get<SInt>(0); set => Set(0, value); }
    public StatSelector<SIntStat> StatSelector { get => Get<StatSelector<SIntStat>>(1); set => Set(1, value); }
}

[TypeIndex(112)]
public class SetStatMinAction : SObj, IAction {
    public SInt NewMin { get => Get<SInt>(0); set => Set(0, value); }
    public StatSelector<SIntStat> StatSelector { get => Get<StatSelector<SIntStat>>(1); set => Set(1, value); }
}
[TypeIndex(150)]
public class NoMaxReason : SObj, IUnusableReason {
    public SString Unit { get => Get<SString>(0); set => Set(0, value); }
}
[TypeIndex(151)]
public class TargetMissingStat : SObj, IResourceSelector<IIntResource> {
    public SEnum<Unit> Unit { get => Get<SEnum<Unit>>(0); set => Set<SEnum<Unit>>(0, value); }
}
[TypeIndex(174)]
public interface IExchangeable : ISerializable {
}
[TypeIndex(175)]
public class SpendCloudsAction : SObj, IAction {
    public IResourceSelector<CloudGroup> SourceSelector { get => Get<IResourceSelector<CloudGroup>>(0); set => Set<IResourceSelector<CloudGroup>>(0, value); }
}
[TypeIndex(176)]
public class CloudGroup : SList<SInt>, IResource {
}
[TypeIndex(177)]
public class MissingCloudReason : SNull, IUnusableReason {
}
[TypeIndex(178)]
public class NoPatternMatchReason : SNull, IUnusableReason {
}
[TypeIndex(179)]
public class ShapeTargetSelector : SObj, IResourceSelector<CloudGroup> {
    public SEnum<Shape> Shape { get => Get<SEnum<Shape>>(0); set => Set(0, value); }
    public SEnum<MagicElement> Element { get => Get<SEnum<MagicElement>>(1); set => Set(1, value); }
}
[TypeIndex(202)]
public class AIBehavior : SingletonStat, ITriggeredStat {
}
[TypeIndex(203)]
public class SkillUseBehavior : AIBehavior {
    public TargetFilterSet TargetPreferences { get => Get<TargetFilterSet>(0); set => Set(0, value); }
}

[TypeIndex(45)]
public class MonsterBehavior : SkillUseBehavior {
}
[TypeIndex(246)]
public interface IAvatarrable : ISerializable {
}
[TypeIndex(55)]
public interface IPanelGenerator : ISerializable {
}

namespace Avatar {
    [SerializableEnum]
    [TypeIndex(83)]
    public enum SkillAvatar : byte {
        Attack,
        Defend,
        Battlecry,
        SmellingSalts,
        Insult,
        Shovel,
        Muertomancy,
        TimeSkip,
        Move,
        // Underscore removes from content tests
        _ThornWhip,
        Grow,
        Entangle,
        Bolas,
        Charge,
        EldritchRegrowth,
        GiftDrop,
        Plant,
        Ignite,
        Pyroclasm,
        Teleport,
        Tumble,
        Shove,
        Twister,
        Rain,
        Summon,
        HealthPotion,
        TimePotion,
        RagePotion,
        SprintPotion,
        AccuracyPotion,
        TemporaryAccuracyPotion,
        TransparentPotion,
        DefensePotion,
        ImmortalPotion,
        AttentionPotion,
        Mortar,
        StudyBlade,
        Claw,
        Suplex,
        Nutmeg,
        Regeneration,
        Headlock,
        Scry,
        Infest,
        Fly,
        Missile,
        Firecracker,
    }
    [SerializableEnum]
    [TypeIndex(44)]
    public enum CharacterAvatar : byte {
        You,
        Fan,
        Spirit,
        DustBunny,
        Mobster,
        GirlScout,
        Vampire,
        Bat,
        Tree,
        Bear,
        Knight,
        Sarge,
        Agatha,
        Wendy,
        Cassandra,
        Gift,
        Paradox,
        Thorn,
        RoseBush,
        IndigoCobra,
        Blaze,
        AfterImage,
        Rock,
        Oak,
        Lion,
        Rhino,
        Professor,
        BlackEagle,
        Shannon,
        Phyllus,
        Cole,
        ZombieRat,
        TheCoven,
        SpookyForest,
    }
    [SerializableEnum]
    [TypeIndex(26)]
    public enum ActionAvatar : byte {
        AddTag,
        AtLeast,
        Compound,
        GainStat,
        Iterative,
        Move,
        MultiTarget,
        Optional,
        PlaceCloud,
        SkillCheck,
        SetStatMax,
        SetStatMin,
        SetTarget,
        Spend,
        SpendCloud,
        StatConditional,
        Summon,
        ToggleStat,
        SetStat,
        ChooseRandom,
        SpendPotion,
    }
    [TypeIndex(247)]
    public class ActionAvatarable : SEnum<ActionAvatar>, IAvatarrable {
    }
    [SerializableEnum]
    [TypeIndex(25)]
    public enum MessageAvatar : byte {
        Vision,
        Error,
        Confusion,
        Trigger,
        Round,
        Skill,
        Upgrades,
        Rules,
    }
    [TypeIndex(248)]
    public class MessageAvatarable : SEnum<MessageAvatar>, IAvatarrable {
    }
}
[TypeIndex(240)]
public class ComicPanel : Model {
    public IPanelGenerator PanelGenerator { get => Get<IPanelGenerator>(1); set => Set(1, value); }
    [HideInInspector]
    public LogStack Log { get => Get<LogStack>(2); set => Set(2, value); }
}
[TypeIndex(241)]
public class PanelPage : Model {
    public SList<ComicPanel> Panels { get => Get<SList<ComicPanel>>(1); set => Set(1, value); }
}
[TypeIndex(242)]
public class ComicBook : Model {
    public static ComicBook Instance;
    public PanelPage LeftPanel { get => Get<PanelPage>(1); set => Set(1, value); }
    public SObj RightPanel { get => Get<SObj>(2); set => Set(2, value); }
    public SInt PageNumber { get => Get<SInt>(3); set => Set(3, value); }
    public SBool AllowPartPage { get => Get<SBool>(4); set => Set(4, value); }
    public SObj LeftPageOverlay { get => Get<SObj>(5); set => Set(5, value); }
    public SInt PanelCount { get => Get<SInt>(6); set => Set(6, value); }
}
[SerializableEnum]
[TypeIndex(46)]
public enum RelativeDirection : byte {
    Front,
    Left,
    Back,
    Right,
}
[TypeIndex(48)]
public class CrittleBearBehavior : SkillUseBehavior {
}
[TypeIndex(52)]
public class ThugAI : MonsterBehavior {
}
[TypeIndex(53)]
public class VampireHealth : HealthStat {
}
[TypeIndex(54)]
public class VampireBatHealth : HealthStat {
    [HideInInspector]
    public SInt VampireHealth { get => Get<SInt>(6); set => Set<SInt>(6, value); }
}
[SerializableEnum]
[TypeIndex(60)]
public enum SkillRarity : byte {
    Minor,
    Major,
}
[TypeIndex(59)]
public class SkillChoice : Model {
    public SList<ShopItem> Options { get => Get<SList<ShopItem>>(1); set => Set(1, value); }
    public SBool First { get => Get<SBool>(2); set => Set(2, value); }
}
[SerializableEnum]
[TypeIndex(61)]
public enum BalanceCategory : byte {
    AttentionProcMultiplier,
    HeelProcMultiplier,
    StartingPlayerHealth,
    BatCount,
    ShuffleBosses,
}
[TypeIndex(62)]
public class BalanceValue : SObj {
    public SEnum<BalanceCategory> Category { get => Get<SEnum<BalanceCategory>>(0); set => Set(0, value); }
    public SInt Value { get => Get<SInt>(1); set => Set(1, value); }
}

[TypeIndex(63)]
public class BalanceAdjustment : SList<BalanceValue> {
}
public interface IStatted {
}
public interface IPathBlocker {
}
public interface IMovementBlocker {
}
public interface IVisionBlocker {
}
[TypeIndex(64)]
public abstract class Character : Tactical, IStatted, IMovementBlocker {
    public SBool Alive { get { return Get<SBool>(2, true); } set { Set(2, value); } }
    public SList<SSkill> Skills { get { return Get<SList<SSkill>>(5); } set { Set(5, value); } }
    public SEnum<Avatar.CharacterAvatar> Avatar { get => Get<SEnum<Avatar.CharacterAvatar>>(6); set => Set(6, value); }
    public virtual SInt2 Target {
        get => Get<SInt2>(7); set {
            Set(7, value);
        }
    }
    public SEnum<CharacterAnimation> ActiveAnimation { get => Get<SEnum<CharacterAnimation>>(9); set => Set(9, value); }
    public SInt LastUsedSkillIndex { get => Get<SInt>(10); set => Set(10, value); }
}
[SerializableEnum]
[TypeIndex(234)]
public enum CharacterAnimation : byte {
    None,
    Attack,
}
[TypeIndex(65)]
[PurgeDataMigration(10, 1)]
public class CharacterInitializationData : SObj {
    public Statblock Statblock { get => Get<Statblock>(2); set => Set(2, value); }
    [PurgeDataMigration(7, 3)]
    public SEnum<Avatar.CharacterAvatar> Avatar { get => Get<SEnum<Avatar.CharacterAvatar>>(3); set => Set(3, value); }
    //public SBool AddBasicAttack { get => Get<SBool>(4); set => Set(4, value); }
    public CharacterSkillTreeProgress SkillProgress { get => Get<CharacterSkillTreeProgress>(5); set => Set(5, value); }
}

[TypeIndex(68)]
public class Monster : Character {
}
[TypeIndex(69)]
public class PlayerCharacter : Character {
}
public interface IPositioned {
}
[TypeIndex(40)]
public abstract class Positioned : Model, IPositioned, IAvatarrable {
}
[TypeIndex(67)]
public abstract class Tactical : Positioned {
}
[TypeIndex(70)]
public class Wall : Positioned, IMovementBlocker, IVisionBlocker, IPathBlocker {
}
[SerializableEnum]
[TypeIndex(79)]
public enum MagicElement : byte {
    Fire,
    Air,
    Plant,
    Water,
    Dust,
    Explosion,
}
public interface IHazardous {
}

[TypeIndex(80)]
// FIXME: maybe not all clouds should block vision?
public class Cloud : Tactical, IVisionBlocker, IHazardous {
    public SEnum<MagicElement> Element { get { return Get<SEnum<MagicElement>>(2); } set { Set(2, value); } }
}
[TypeIndex(172)]
[SerializableEnum]
public enum GameConcept : byte {
    Cloud,
    Chance,
    Probability,
    Max,
    Summon,
    Temporary,
}
[TypeIndex(71)]
public interface IIntention : IAssemblyType, ISerializable, IAvatarrable {
}

[TypeIndex(251)]
public class Symbol : SObj {
    public SString Name { get => Get<SString>(0); set => Set(0, value); }
    public ISerializable Value { get => Get<ISerializable>(1); set => Set(1, value); }
}

[TypeIndex(72)]
public class Intention : SObj, IIntention {
    // These need to be references instead of full objects.
    [HideInInspector]
    public SInt OriginatingCharacterIndex { get => Get<SInt>(0); set => Set(0, value); }
    [HideInInspector]
    public SInt2 Target { get => Get<SInt2>(1); set => Set(1, value); }
    [HideInInspector]
    public SInt SkillIndex { get => Get<SInt>(2); set => Set(2, value); }
}
[TypeIndex(81)]
public class CharacterSkillTreeProgress : SObj {
    public SList<SList<SInt>> SkillUpgradeIndicies { get => Get<SList<SList<SInt>>>(0); set => Set(0, value); }
    public SList<SInt> SkillIds { get => Get<SList<SInt>>(1); set => Set(1, value); }
    public SList<SEnum<PanelIdentifier>> SkillPanelOverrides { get => Get<SList<SEnum<PanelIdentifier>>>(2); set => Set(2, value); }
}

[TypeIndex(82)]
public class LossScreen : Model {
}
[TypeIndex(84)]
public class ShopItem : Model {
    public SSkill Skill { get => Get<SSkill>(1); set => Set(1, value); }
    public SInt PotionIndex { get => Get<SInt>(2); set => Set(2, value); }
    public SInt Cost { get => Get<SInt>(3); set => Set(3, value); }
    public SInt SkillId { get => Get<SInt>(4); set => Set(4, value); }
    public SInt UpgradeIndex { get => Get<SInt>(5); set => Set(5, value); }
    public SString Title { get => Get<SString>(6); set => Set(6, value); }
    public SString Subtitle { get => Get<SString>(7); set => Set(7, value); }
    public LogStack Body { get => Get<LogStack>(8); set => Set(8, value); }
    public SLong ModelRelationship { get => Get<SLong>(9); set => Set(9, value); }
}
[TypeIndex(85)]
public class Neighborhood : Model {
    public SInt LeftBossIndex { get => Get<SInt>(2); set => Set(2, value); }
    public SInt CenterBossIndex { get => Get<SInt>(3); set => Set(3, value); }
    public SInt RightBossIndex { get => Get<SInt>(4); set => Set(4, value); }
    public SBool HasLeftBoss { get => Get<SBool>(8); set => Set(8, value); }
    public SBool HasRightBoss { get => Get<SBool>(9); set => Set(9, value); }
}

[TypeIndex(58)]
public class Shop : Model {
    public SList<ShopItem> Options { get => Get<SList<ShopItem>>(1); set => Set(1, value); }
    public SInt Points { get => Get<SInt>(2); set => Set(2, value); }
}
[TypeIndex(86)]
public class SkillTree : SObj {
    public SSkill BaseSkill { get => Get<SSkill>(0); set => Set(0, value); }
    public SList<SkillUpgrade> Upgrades { get => Get<SList<SkillUpgrade>>(1); set => Set(1, value); }
    public SBool PlayerSkill { get => Get<SBool>(2); set => Set(2, value); }
}
[SerializableEnum]
[TypeIndex(87)]
public enum ChangeType : byte {
    Replace,
    Add,
    Remove
}

[TypeIndex(88)]
public class SkillEdit : SObj {
    [IntToLongSObjMigration(1, 0)]
    public SLong PropertyIndex { get => Get<SLong>(0); set => Set(0, value); }
    public ISerializable NewValue { get => Get<ISerializable>(1); set => Set(1, value); }
    public SEnum<ChangeType> EditType { get => Get<SEnum<ChangeType>>(2); set => Set(2, value); }
}
[TypeIndex(214)]
public class UpgradeId : SObj {
    public SInt SkillIndex { get => Get<SInt>(0); set => Set(0, value); }
    public SInt UpgradeIndex { get => Get<SInt>(1); set => Set(1, value); }
}

[TypeIndex(89)]
public class SkillUpgrade : SObj {
    public SList<SkillEdit> Changes { get => Get<SList<SkillEdit>>(0); set => Set(0, value); }
    public SString Name { get => Get<SString>(1); set => Set(1, value); }
    public SEnum<SkillRarity> Rarity { get => Get<SEnum<SkillRarity>>(2); set => Set(2, value); }
    public SList<SInt> PrerequisiteSkillIds { get => Get<SList<SInt>>(3); set => Set(3, value); }
    [PurgeDataMigration(9, 4)]
    public SList<UpgradeId> PrerequisiteUpgrades { get => Get<SList<UpgradeId>>(4); set => Set(4, value); }
    [PurgeDataMigration(9, 5)]
    public SList<UpgradeId> IncompatibleUpgrades { get => Get<SList<UpgradeId>>(5); set => Set(5, value); }
    public SString Description { get => Get<SString>(6); set => Set(6, value); }
}
[TypeIndex(90)]
public class WinScreen : Model {
    public SBool IsRunOver { get => Get<SBool>(1); set => Set(1, value); }
    public SBool CanSkipShop { get => Get<SBool>(2); set => Set(2, value); }
    public SInt HeroPoints { get => Get<SInt>(3); set => Set(3, value); }
    public SInt FightBonus { get => Get<SInt>(4); set => Set(4, value); }
    public SInt PointDelta { get => Get<SInt>(5); set => Set(5, value); }
}
[TypeIndex(259)]
public class SkillId : SInt {
}

[TypeIndex(260)]
public class PlayerCharacterModel : SObj {
    public SEnum<Avatar.CharacterAvatar> AvatarOverride { get => Get<SEnum<Avatar.CharacterAvatar>>(0); set => Set(0, value); }
    public SList<SkillId> SkillIndicies { get => Get<SList<SkillId>>(1); set => Set(1, value); }
    public SEnum<PanelIdentifier> IntroPanel { get => Get<SEnum<PanelIdentifier>>(2); set => Set(2, value); }
}
[SerializableEnum]
[TypeIndex(93)]
public enum GameProgress : byte {
    InProgress,
    Failed,
    Succeeded,
    None,
}

[SerializableEnum]
[TypeIndex(237)]
public enum RewardType : byte {
    EasyFight,
    NormalFight,
    HardFight,
    Victory,
}
[TypeIndex(92)]
public class SSkill : Model, IAvatarrable {
    [HideInInspector]
    public SList<IIntention> Intentions { get => Get<SList<IIntention>>(1); protected set => Set(1, value); }
    public CompoundAction Action { get => Get<CompoundAction>(2); set => Set(2, value); }
    [HideInInspector]
    public SList<SInt> Upgrades { get => Get<SList<SInt>>(3); set => Set(3, value); }
    [PurgeDataMigration(7, 4)]
    public SEnum<Avatar.SkillAvatar> Avatar { get => Get<SEnum<Avatar.SkillAvatar>>(4); set => Set(4, value); }
    public SEnum<PanelIdentifier> PanelIdentifier { get => Get<SEnum<PanelIdentifier>>(5); set => Set(5, value); }
    [HideInInspector]
    public LogStack Description { get => Get<LogStack>(7); set => Set(7, value); }
    [HideInInspector]
    public SString RejectMessage { get => Get<SString>(8); set => Set(8, value); }
    [ReplaceWithNullMigration(2, 9)]
    public IResourceSelector<IIntResource> RangeSelector { get => Get<IResourceSelector<IIntResource>>(9); set => Set(9, value); }
    public CompoundAction InitializationAction { get => Get<CompoundAction>(10); set => Set(10, value); }
    public SEnum<CharacterAnimation> Animation { get => Get<SEnum<CharacterAnimation>>(11); set => Set(11, value); }
    [HideInInspector]
    public SBool Highlighted { get => Get<SBool>(12); set => Set(12, value); }
    [HideInInspector]
    public LogStack NPCDescription { get => Get<LogStack>(13); set => Set(13, value); }
    public SList<Symbol> Symbols { get => Get<SList<Symbol>>(14); set => Set(14, value); }
    [HideInInspector]
    public SBool ShowPercentage { get => Get<SBool>(15); set => Set(15, value); }
    [HideInInspector]
    public SInt Percentage { get => Get<SInt>(16); set => Set(16, value); }
    public SList<IBoolSelector> UsabilityRequirements { get => Get<SList<IBoolSelector>>(17); set => Set(17, value); }
}
[TypeIndex(249)]
public class Table : Tactical {
}
[TypeIndex(99)]
public class TacticalScene : Model {
    public SList<Tactical> Pieces { get => Get<SList<Tactical>>(2); set => Set(2, value); }
    public SInt TurnOrderIndex {
        get => Get<SInt>(5);
        set {
            Set(5, value);
        }
    }
    public SEnum<GameProgress> Progress {
        get => Get<SEnum<GameProgress>>(8);
        set {
            Set(8, value);
        }
    }
    public SBool ShouldClose { get => Get<SBool>(9); set => Set(9, value); }
}
[TypeIndex(96)]
public class Background : Model, IPositioned {
    public SInt2 Position { get => Get<SInt2>(1); set => Set(1, value); }
    // FIXME: this is key 3 because that's what it is in character, and they share positioned text view
    public SBool Visible { get => Get<SBool>(3); set => Set(3, value); }
    public SBool Highlighted { get => Get<SBool>(4); set => Set(4, value); }
}
[TypeIndex(97)]
public interface IUnusableReason : ISerializable {
}

[ProtectStatics]
[TypeIndex(98)]
public class UnusableReasons : SList<IUnusableReason> {
}
[SerializableEnum]
[TypeIndex(56)]
public enum PanelIdentifier : byte {
    Unimplemented,
    HideSuccess,
    HideFail,
    NoPossibleAction,
    WindUp,
    VampireDisappear,
    VampireReform,
    StartTurn,
    _ChooseEnemy,
    FireExtinguished,
    Win,
    Lose,
    PressToContinue,
    _Moved,
    DrinkPotion,
    Summon,
    DazeStunned,
    DazeNotStunned,
    HeelSuccess,
    HeelFail,
    AttentionSuccess,
    AttentionFail,
    Dizzy,
    ImmortalProc,
    FanProtection,
    ResistConstrictDamageSuccess,
    ResistConstrictDamageFail,
    Death,
    RageSuccess,
    RageFail,
    Stun,
    ProcSpaced,
    _GetEnvelope,
    _OpenEnvelope,
    CharacterCreate,
    SkillAttack,
    SkillBattlecry,
    SkillBolas,
    SkillDefend,
    SkillRegrowth,
    SkillEntangle,
    SkillGrow,
    SkillIgnite,
    SkillInsult,
    SkillMortar,
    SkillMove,
    SkillMuertomancy,
    SkillPlant,
    SkillPyroclasm,
    SkillShove,
    SkillShovel,
    SkillSmellingSalts,
    SkillSummon,
    SkillTeleport,
    SkillTimeSkip,
    SkillStudyBlade,
    ChooseBeagle,
    WalkBeagle,
    ChooseShannon,
    WalkShannon,
    ChooseCole,
    WalkCole,
    ChoosePhillis,
    WalkPhillis,
    Mobster,
    GirlScout,
    Thorn,
    Vampire,
    IndigoCobra,
    Professor,
    Sarge,
    CrittleBear,
    Rock,
    Witch,
    Oak,
    Blaze,
    SkillClaw,
    SkillSuplex,
    Embarrassed,
    Regenerating,
    PelletsShop,
    RiskBattle,
}
[TypeIndex(57)]
public class PanelParamBlueprint : SObj, IPanelGenerator {
    public SEnum<PanelIdentifier> PanelId { get => Get<SEnum<PanelIdentifier>>(0); set => Set(0, value); }
    public SList<ISerializable> Arguments { get => Get<SList<ISerializable>>(1); set => Set(1, value); }
    public SInt PanelDatabaseIndex { get => Get<SInt>(2); set => Set(2, value); }
}
[TypeIndex(100)]
public interface IAction : ISerializable, IAvatarrable {
}
[TypeIndex(101)]
public class NullAction : SNull, IAction {
}
[TypeIndex(102)]
public class AddTagAction : SObj, IAction {
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(0); set => Set(0, value); }
    public SEnum<Tags> Tag { get => Get<SEnum<Tags>>(1); set => Set(1, value); }
}
[TypeIndex(103)]
public class AtLeast : SObj, IResourceSelector<IBoolResource> {
    public IResourceSelector<IIntResource> LeftHandSide { get { return Get<IResourceSelector<IIntResource>>(0); } set { Set<IResourceSelector<IIntResource>>(0, value); } }
    public IResourceSelector<IIntResource> RightHandSide { get { return Get<IResourceSelector<IIntResource>>(1); } set { Set<IResourceSelector<IIntResource>>(1, value); } }
}
[TypeIndex(155)]
public class BranchAction : SObj, IAction {
    public IAction TrueAction { get => Get<IAction>(0); set => Set(0, value); }
    public IAction FalseAction { get => Get<IAction>(1); set => Set(1, value); }
    public IResourceSelector<IBoolResource> ConditionSelector { get => Get<IResourceSelector<IBoolResource>>(2); set => Set(2, value); }
}
[TypeIndex(233)]
public class ChooseRandomAction : SObj, IAction {
    public SList<IAction> Actions { get => Get<SList<IAction>>(0); set => Set(0, value); }
}
[TypeIndex(104)]
public class CompoundAction : SList<IAction>, IAction {
}
[TypeIndex(266)]
public class ConstantBool : SObj, IBoolSelector {
    public SBool Value { get => Get<SBool>(0); set => Set(0, value); }
}
[TypeIndex(258)]
public class IntEquals : SObj, IBoolSelector {
    public IResourceSelector<IIntResource> LeftHandSide { get => Get<IResourceSelector<IIntResource>>(1); set => Set(1, value); }
    public IResourceSelector<IIntResource> RightHandSide { get => Get<IResourceSelector<IIntResource>>(2); set => Set(2, value); }
}
[TypeIndex(105)]
public class IterativeAction : SObj, IAction {
    public IAction Subaction { get => Get<IAction>(0); set => Set<IAction>(0, value); }
    public IResourceSelector<IIntResource> RepeatSelector { get => Get<IResourceSelector<IIntResource>>(1); set => Set<IResourceSelector<IIntResource>>(1, value); }
    public SBool ConsumeResource { get => Get<SBool>(2); set => Set<SBool>(2, value); }
    public SBool RequireResource { get => Get<SBool>(3); set => Set<SBool>(3, value); }
}
[TypeIndex(106)]
public class MoveAction : SObj, IAction {
    public IResourceSelector<IValuedResource<SInt2>> DestinationSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(1); set => Set(1, value); }
    public IResourceSelector<IValuedResource<SInt2>> MoverSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(2); set => Set(2, value); }
}
[TypeIndex(107)]
public class MultiTargetAction : SObj, IAction {
    public IAction Subaction { get => Get<IAction>(0); set => Set<IAction>(0, value); }
    public TargetFilterSet Filter { get => Get<TargetFilterSet>(1); set => Set<TargetFilterSet>(1, value); }
}
[TypeIndex(263)]
public class OnEdge : SObj, IBoolSelector {
    public IResourceSelector<IValuedResource<SInt2>> PositionProvider { get => Get<IResourceSelector<IValuedResource<SInt2>>>(0); set => Set(0, value); }
}
[TypeIndex(108)]
public class OptionalAction : SObj, IAction {
    public IAction Subaction { get => Get<IAction>(0); set => Set(0, value); }
}
[TypeIndex(109)]
public class PlaceCloudAction : SObj, IAction {
    public SEnum<MagicElement> Element { get => Get<SEnum<MagicElement>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<SInt2>> TargetSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(1); set => Set(1, value); }
    public SEnum<Shape> Shape { get => Get<SEnum<Shape>>(2); set => Set(2, value); }
}
[TypeIndex(110)]
public class SkillCheck : SObj, IResourceSelector<IBoolResource> {
    public IResourceSelector<IIntResource> ModifierSource { get { return Get<IResourceSelector<IIntResource>>(0); } set { Set<IResourceSelector<IIntResource>>(0, value); } }
}
[TypeIndex(211)]
public class RandomChance : SObj, IResourceSelector<IBoolResource> {
    public IResourceSelector<IIntResource> ModifierSource { get { return Get<IResourceSelector<IIntResource>>(0); } set { Set<IResourceSelector<IIntResource>>(0, value); } }
}
[TypeIndex(217)]
public class RemoveTagAction : SObj, IAction {
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(0); set => Set(0, value); }
    public SEnum<Tags> Tag { get => Get<SEnum<Tags>>(1); set => Set(1, value); }
}
[TypeIndex(111)]
public class SetStatMaxAction : SObj, IAction {
    public SInt NewMax { get => Get<SInt>(0); set => Set(0, value); }
    public StatSelector<SIntStat> StatSelector { get => Get<StatSelector<SIntStat>>(1); set => Set(1, value); }
}
[TypeIndex(261)]
public class SetSymbolAction<T> : SObj, IAction where T : ISerializable {
    public SymbolSelector<T> SymbolSelector { get => Get<SymbolSelector<T>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<T>> ValueSelector { get => Get<IResourceSelector<IValuedResource<T>>>(1); set => Set(1, value); }
}
[TypeIndex(113)]
public class SetTargetAction : SObj, IAction {
    public IResourceSelector<IValuedResource<SInt2>> TargetSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(0); set => Set(0, value); }
}
[TypeIndex(114)]
public class SpendAction : SObj, IAction {
    public IResourceSelector<IIntResource> SourceSelector { get => Get<IResourceSelector<IIntResource>>(0); set => Set(0, value); }
    public IResourceSelector<IIntResource> AmountSelector { get => Get<IResourceSelector<IIntResource>>(1); set => Set(1, value); }
    public SBool IsDamage { get => Get<SBool>(2); set => Set(2, value); }
}
[TypeIndex(219)]
public class SpendDynamicAction : SObj, IAction {
    public IResourceSelector<IIntResource> SourceSelector { get => Get<IResourceSelector<IIntResource>>(0); set => Set(0, value); }
    public IResourceSelector<IIntResource> AmountSelector { get => Get<IResourceSelector<IIntResource>>(1); set => Set(1, value); }
    public SBool IsDamage { get => Get<SBool>(2); set => Set(2, value); }
}
[TypeIndex(236)]
public class SpendPotionAction : SObj, IAction {
}
[TypeIndex(115)]
public class ConditionalAction : SObj, IAction {
    public IAction Action { get => Get<IAction>(0); set => Set(0, value); }
    public IResourceSelector<IBoolResource> ConditionSelector { get => Get<IResourceSelector<IBoolResource>>(1); set => Set(1, value); }
}
[TypeIndex(116)]
public class SummonAction : SObj, IAction {
    public SEnum<Avatar.CharacterAvatar> Identifier { get => Get<SEnum<Avatar.CharacterAvatar>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<SInt2>> PositionSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(1); set => Set(1, value); }
    public CompoundAction PostSummonAction { get => Get<CompoundAction>(2); set => Set(2, value); }
}
[TypeIndex(117)]
public class ToggleStatAction : SObj, IAction {
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(0); set => Set(0, value); }
    public IEquatableStat Stat { get => Get<IEquatableStat>(1); set => Set(1, value); }
}
[SerializableEnum]
[TypeIndex(120)]
public enum Shape : byte {
    Spot,
    Line2,
    Line3,
    Cross3,
    Cross5,
    X3,
    X5,
    Square2,
    Square3,
    Block3,
}

[TypeIndex(121)]
public class ShapeCell : SObj {
    public SBool Filled { get => Get<SBool>(0); set => Set(0, value); }
    public SBool Unimportant { get => Get<SBool>(1); set => Set(1, value); }
    public SBool Outcome { get => Get<SBool>(2); set => Set(2, value); }
}
[TypeIndex(122)]
public class SShape : SObj {
    public SInt Size { get => Get<SInt>(0); set => Set(0, value); }
    public SList<ShapeCell> Data { get => Get<SList<ShapeCell>>(1); set => Set(1, value); }
}
[SerializableEnum]
[TypeIndex(123)]
public enum TargetFilter : byte {
    Enemy,
    Ally,
    Closest,
    WithinRange,
    AtExactRange,
    Furthest,
    DirectPathExists,
    Aligned,
    HighestHealth,
    LowestHealth,
    Unoccupied,
    NonAlly,
    Forward,
    Adjacent,
    Self,
    NotSelf,
    AwayFromEnemy,
    Spirit,
    Constricted,
    Edge,
    Character,
    Unblocked,
    // Oops.  This is the same as aligned
    StraightLine,
    NonAdjacent,
    NoCloud,
}
[TypeIndex(124)]
public class NoTargetedCharacterReason : SNull, IUnusableReason {
    public string Interpertation => "no targeted character";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(125)]
public class TargetNotEnemyReason : SNull, IUnusableReason {
    public string Interpertation => "target is not an enemy";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(126)]
public class TargetOutOfRangeReason : SNull, IUnusableReason {
    public string Interpertation => "target out of range";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(127)]
public class TargetOccupiedReason : SNull, IUnusableReason {
    public string Interpertation => "target isn't empty";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(128)]
public class TargetAllyReason : SNull, IUnusableReason {
    public string Interpertation => "target is an ally";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(129)]
public class TargetNotAllyReason : SNull, IUnusableReason {
    public string Interpertation => "target isn't an ally";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(130)]
public class TargetNotInFrontReason : SNull, IUnusableReason {
    public string Interpertation => "target isn't in front";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(131)]
public class TargetNotSelfReason : SNull, IUnusableReason {
    public string Interpertation => "must target self";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(132)]
public class TargetSelfReason : SNull, IUnusableReason {
    public string Interpertation => "cannot target self";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(133)]
public class TargetTowardsEnemyReason : SNull, IUnusableReason {
    public string Interpertation => "target is toward an enemy";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(204)]
public class MovingImmobileReason : SNull, IUnusableReason {
    public string Interpertation => "target cannot move";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(209)]
public class TargetNotSpiritReason : SNull, IUnusableReason {
    public string Interpertation => "target isn't a spirit";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(21)]
public class TargetNotConstrictedReason : SNull, IUnusableReason {
    public string Interpertation => "target isn't constricted";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(119)]
public class TargetNotEdgeReason : SNull, IUnusableReason {
    public string Interpertation => "target isn't on the edge";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(227)]
public class TargetNotCharacterReason : SNull, IUnusableReason {
    public string Interpertation => "no target character";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(38)]
public class TargetBlockedReason : SNull, IUnusableReason {
    public string Interpertation => "target occupied";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(239)]
public class TargetDiagonalReason : SNull, IUnusableReason {
    public string Interpertation => "target not a straight line";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(50)]
public class TargetAdjacentReason : SNull, IUnusableReason {
    public string Interpertation => "target too close";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(51)]
public class TargetHasCloudReason : SNull, IUnusableReason {
    public string Interpertation => "target already contains a cloud";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(47)]
public class NoValidTargetsReason : SNull, IUnusableReason {
    public string Interpertation => "no valid targets";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(264)]
public class NotOnEdgeReason : SNull, IUnusableReason {
    public string Interpertation => "not on the edge of the arena";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(267)]
public class ConstantBoolReason : SNull, IUnusableReason {
    public string Interpertation => "you can't use this";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}

[TypeIndex(134)]
public class TargetFilterSet : SList<SEnum<TargetFilter>> {
}
[TypeIndex(207)]
public class CharacterCountSelector : SObj, IResourceSelector<IIntResource> {
    public TargetFilterSet Filter { get => Get<TargetFilterSet>(0); set => Set(0, value); }
}
[TypeIndex(224)]
public class CharacterDirection : SObj, IResourceSelector<IValuedResource<SEnum<CardinalDirection>>> {
    public IResourceSelector<IValuedResource<Character>> CharacterSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(0); set => Set(0, value); }
}
[TypeIndex(221)]
public class CharacterLastPosition : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(0); set => Set(0, value); }
}

[TypeIndex(220)]
public class ConditionalIntSelector : SObj, IResourceSelector<IIntResource> {
    public IResourceSelector<IBoolResource> Condition { get => Get<IResourceSelector<IBoolResource>>(0); set => Set(0, value); }
    public IResourceSelector<IIntResource> WhenTrue { get => Get<IResourceSelector<IIntResource>>(1); set => Set(1, value); }
    public IResourceSelector<IIntResource> WhenFalse { get => Get<IResourceSelector<IIntResource>>(2); set => Set(2, value); }
}
[TypeIndex(135)]
public class ConstantIntSelector : SObj, IResourceSelector<IIntResource> {
    public SInt Value { get => Get<SInt>(0); set => Set(0, value); }
}
[TypeIndex(225)]
public class DistanceFromCharacter : SObj, IResourceSelector<IIntResource> {
    public IResourceSelector<IValuedResource<Character>> CharacterSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<SInt2>> PositionSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(1); set => Set(1, value); }
}
[TypeIndex(136)]
public class IntStatSelector : SObj, IResourceSelector<IIntResource> {
    public SEnum<Unit> Unit { get => Get<SEnum<Unit>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(1); set => Set(1, value); }
}
[TypeIndex(137)]
public class InvertedIntSelector : SObj, IResourceSelector<IIntResource> {
    public IResourceSelector<IIntResource> Subselector { get => Get<IResourceSelector<IIntResource>>(0); set => Set(0, value); }
}
[TypeIndex(138)]
public class IntSumSelector : SList<IResourceSelector<IIntResource>>, IResourceSelector<IIntResource> {
}
[TypeIndex(231)]
public class IsInDirection : SObj, IResourceSelector<IBoolResource> {
    public IResourceSelector<IValuedResource<SInt2>> FromTileSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<SInt2>> ToTileSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(1); set => Set(1, value); }
    public IResourceSelector<IValuedResource<SEnum<CardinalDirection>>> DirectionSelector { get => Get<IResourceSelector<IValuedResource<SEnum<CardinalDirection>>>>(2); set => Set(2, value); }
}
[TypeIndex(254)]
public class WrongLastSkillReason : SObj, IUnusableReason {
    public string Interpertation => "only usable after " + Avatar.Value;
    public SEnum<Avatar.SkillAvatar> Avatar;
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}
[TypeIndex(255)]

public class LastSkillEquals : SObj, IBoolSelector {
    public SEnum<Avatar.SkillAvatar> Avatar { get => Get<SEnum<Avatar.SkillAvatar>>(1); set => Set(1, value); }
}
[TypeIndex(232)]
public class OppositeDirection : SObj, IResourceSelector<IValuedResource<SEnum<CardinalDirection>>> {
    public IResourceSelector<IValuedResource<SEnum<CardinalDirection>>> DirectionSelector { get => Get<IResourceSelector<IValuedResource<SEnum<CardinalDirection>>>>(0); set => Set(0, value); }
}
[TypeIndex(139)]
public class OriginatorSelector : SNull, IResourceSelector<IValuedResource<Character>> {
}
[TypeIndex(140)]
public class PathLengthReason : SObj, IUnusableReason {
    protected SInt NeededDistance { get => Get<SInt>(0); set => Set(0, value); }
    protected SInt ActualDistance { get => Get<SInt>(1); set => Set(1, value); }
}
[TypeIndex(141)]
public class PathTileSelector : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public IResourceSelector<IValuedResource<SInt2>> PathStartSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<SInt2>> PathEndSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(1); set => Set(1, value); }
    public IResourceSelector<IIntResource> DistanceSelector { get => Get<IResourceSelector<IIntResource>>(2); set => Set(2, value); }
}
[TypeIndex(142)]
public class RelativePositionSelector : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public SEnum<RelativeDirection> Direction { get => Get<SEnum<RelativeDirection>>(0); set => Set(0, value); }
}
[TypeIndex(208)]
public class CharacterRelativePositionSelector : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public SEnum<RelativeDirection> Direction { get => Get<SEnum<RelativeDirection>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<Character>> CharacterSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(1); set => Set(1, value); }
}
[TypeIndex(262)]
public class PositionedCharacterSelector : SObj, IResourceSelector<IValuedResource<Character>> {
    public IResourceSelector<IValuedResource<SInt2>> PositionSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(0); set => Set(0, value); }
}
[TypeIndex(223)]
public class PositionPlusDirection : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public IResourceSelector<IValuedResource<SInt2>> PositionSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<SEnum<CardinalDirection>>> DirectionSelector { get => Get<IResourceSelector<IValuedResource<SEnum<CardinalDirection>>>>(1); set => Set(1, value); }
}
[TypeIndex(228)]
public class PositionsEqual : SObj, IResourceSelector<IBoolResource> {
    public IResourceSelector<IValuedResource<SInt2>> FirstTileSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<SInt2>> SecondTileSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(1); set => Set(1, value); }
}
[TypeIndex(143)]
public class RandomCharacter : SObj, IResourceSelector<IValuedResource<Character>> {
    public TargetFilterSet Filter { get => Get<TargetFilterSet>(0); set => Set(0, value); }
}
[TypeIndex(144)]
public class NoPossibleTilesReason : SNull, IUnusableReason {
    public string Interpertation => "there aren't any possible tiles";
    public PanelIdentifier ErrorPanel => PanelIdentifier.Unimplemented;
}

[TypeIndex(145)]
public class RandomNearbyTile : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public SInt MaxDistance { get => Get<SInt>(0); set => Set(0, value); }
    public SInt MinDistance { get => Get<SInt>(1); set => Set(1, value); }
    public TargetFilterSet Filter { get => Get<TargetFilterSet>(2); set => Set(2, value); }
}
[TypeIndex(222)]
public class RandomTile : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public TargetFilterSet Filter { get => Get<TargetFilterSet>(0); set => Set(0, value); }
}
[TypeIndex(146)]
public class RandomTileNearTarget : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public SInt MaxDistance { get => Get<SInt>(0); set => Set(0, value); }
    public SInt MinDistance { get => Get<SInt>(1); set => Set(1, value); }
    public IResourceSelector<IValuedResource<SInt2>> TargetSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(2); set => Set(2, value); }
    public TargetFilterSet Filter { get => Get<TargetFilterSet>(3); set => Set(3, value); }
}
[TypeIndex(147)]
public class StatSelector<T> : SObj, IResourceSelector<T> where T : class, IStackableStat {
    public SEnum<Unit> Unit { get => Get<SEnum<Unit>>(0); set => Set<SEnum<Unit>>(0, value); }
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(1); set => Set<IResourceSelector<IValuedResource<Character>>>(1, value); }
}
[TypeIndex(250)]
public class SymbolSelector<T> : SObj, IResourceSelector<IValuedResource<T>> where T : ISerializable {
    public SString SymbolName { get => Get<SString>(0); set => Set(0, value); }
}
[TypeIndex(148)]
public class TargetCharacterSelector : SObj, IResourceSelector<IValuedResource<Character>> {
    public TargetFilterSet Filter { get => Get<TargetFilterSet>(0); set => Set(0, value); }
}
[TypeIndex(149)]
public class TargetHasStat : SObj, IResourceSelector<IBoolResource> {
    public SEnum<Unit> Unit { get => Get<SEnum<Unit>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(1); set => Set<IResourceSelector<IValuedResource<Character>>>(1, value); }
}
[TypeIndex(215)]
public class TargetHasTag : SObj, IResourceSelector<IBoolResource> {
    public SEnum<Tags> Tag { get => Get<SEnum<Tags>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(1); set => Set<IResourceSelector<IValuedResource<Character>>>(1, value); }
}
[TypeIndex(152)]
public class CharacterPosition : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public IResourceSelector<IValuedResource<Character>> TargetSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(0); set => Set(0, value); }
}
[TypeIndex(153)]
public class TargetPositionSelector : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public TargetFilterSet Filter { get => Get<TargetFilterSet>(0); set => Set(0, value); }
}
[TypeIndex(154)]
public class TileContainsCloud : SObj, IResourceSelector<IBoolResource> {
    public SEnum<MagicElement> Element { get => Get<SEnum<MagicElement>>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<SInt2>> TileSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(1); set => Set(1, value); }
}
[TypeIndex(156)]
public class UnoccupiedPositionSelector : SObj, IResourceSelector<IValuedResource<SInt2>> {
    public IResourceSelector<IValuedResource<SInt2>> TileSelector { get => Get<IResourceSelector<IValuedResource<SInt2>>>(0); set => Set(0, value); }
    public SBool IncludeNonBlockers { get => Get<SBool>(1); set => Set(1, value); }
}
[TypeIndex(157)]
public enum ActionEvent : byte {
    Initialize,
    StartTurn,
    BeginAction,
    EndAction,
    EndTurn,
    Death,
    DealDamage,
    DealtDamage,
    Moved,
    OverlapBegin,
    OverlapEnd,
}

[TypeIndex(158)]
public class SelfTargetedIntention : SObj, IIntention {
    [HideInInspector]
    public SInt OriginatingCharacterIndex { get => Get<SInt>(0); set => Set<SInt>(0, value); }
    [HideInInspector]
    public UnusableReasons UnusableReasons { get => Get<UnusableReasons>(1); set => Set(1, value); }
}
[TypeIndex(159)]
public class ActionStat : SObj, IStat, IEquatableStat, ITriggeredStat, IContextualStat, IInitializableStat {
    [HideInInspector]
    public SInt ID { get => Get<SInt>(0); set => Set<SInt>(0, value); }
    public SEnum<ActionEvent> Event { get => Get<SEnum<ActionEvent>>(1); set => Set<SEnum<ActionEvent>>(1, value); }
    public IAction Action { get => Get<IAction>(2); set => Set<IAction>(2, value); }
    [HideInInspector]
    public IIntention Context { get => Get<IIntention>(3); set => Set<IIntention>(3, value); }
    [HideInInspector]
    public SInt OwnerIndex { get => Get<SInt>(4); set => Set(4, value); }
}
[TypeIndex(160)]
public class GainStatAction : SObj, IAction {
    public IStat Stat { get => Get<IStat>(0); set => Set(0, value); }
    public IResourceSelector<IValuedResource<Character>> RecipientSelector { get => Get<IResourceSelector<IValuedResource<Character>>>(1); set => Set(1, value); }
}
[TypeIndex(118)]
public class AgeStat : SIntStat, ITriggeredStat {
}
[TypeIndex(245)]
public class AttackResistantStat : TacticalTag, ITriggeredStat {
}
[TypeIndex(161)]
public class AttentionStat : SIntStat, IInitializableStat {
}
[TypeIndex(218)]
public class BarbStat : SIntStat {
}
[TypeIndex(257)]
public class BleedStat : SIntStat, ITriggeredStat {
}
[TypeIndex(162)]
public class UnitBoundMod : SObj {
    public SEnum<Unit> Unit { get => Get<SEnum<Unit>>(0); set => Set(0, value); }
    public SBool Max { get => Get<SBool>(1); set => Set(1, value); }
    public SInt Value { get => Get<SInt>(2); set => Set(2, value); }
}

[TypeIndex(163)]
public class BoundStat : SList<UnitBoundMod>, IEquatableStat {
}
[TypeIndex(205)]
public class ConfusedStat : SIntStat {
    public AIBehavior PreviousBehavior { get => Get<AIBehavior>(6); set => Set(6, value); }
}
[TypeIndex(206)]
public class ConfusionBehavior : AIBehavior {
}
[TypeIndex(226)]
public class DamageAdjacentStat : TacticalTag, ITriggeredStat {
}
[TypeIndex(243)]
public class DamageOverlapStat : TacticalTag, ITriggeredStat {
}
[TypeIndex(164)]
public class DazeStat : SIntStat {
}
[TypeIndex(210)]
public class DefenseStat : SIntStat {
}
[TypeIndex(49)]
public class DizzyStat : SIntStat {
}
[TypeIndex(265)]
public class EmbarrassedStat : TacticalTag, ITriggeredStat {
}
[TypeIndex(165)]
public class EnumStat<T> : SEnum<T>, IEquatableStat where T : Enum {
}

[TypeIndex(166)]
public class Facing : EnumStat<CardinalDirection>, IInitializableStat { }
[TypeIndex(167)]
public class FadingStat : SIntStat {
}
[TypeIndex(168)]
public class HealthStat : SIntStat {
}
[TypeIndex(244)]
public class HeavyHitterStat : TacticalTag, ITriggeredStat {
}
[TypeIndex(169)]
public class ImmortalStat : SIntStat {
}
[TypeIndex(170)]
public class SIntStat : Model, ITriggeredStat, IStackableStat, IIntResource, IInitializableStat, IViewableStat, ITooltipParent<LogStack> {
    public virtual SInt Value {
        get => Get<SInt>(1);
        set {
            Set<SInt>(1, value);
        }
    }
    public SEnum<Unit> unit { get => Get<SEnum<Unit>>(2); set => Set(2, value); }
    [HideInInspector]
    public SInt CharacterIndex { get => Get<SInt>(3); set => Set(3, value); }
    [HideInInspector]
    public virtual IRange ValueRange { get => Get<Range>(4); set => Set(4, value); }
    [HideInInspector]
    public LogStack TooltipChildren { get => Get<LogStack>(5); set => Set(5, value); }
}
[TypeIndex(235)]
public class OverlapObscure : TacticalTag, ITriggeredStat {
}
[TypeIndex(229)]
public class ProneStat : SIntStat {
}
[TypeIndex(171)]
public class RageStat : SIntStat, IInitializableStat {
}
[TypeIndex(230)]
public class ReflexShoveStat : TacticalTag, ITriggeredStat {
}
[TypeIndex(269)]
public class RegeneratingStat : TacticalTag, ITriggeredStat {
}
[TypeIndex(173)]
public interface IResourceSelector<T> : ISerializable where T : IResource {
}
[TypeIndex(256)]
public interface IBoolSelector : IResourceSelector<IBoolResource> {
}
[TypeIndex(268)]
public class ScryingStat : SIntStat, ITriggeredStat {
}
[TypeIndex(180)]
public class Statblock : SObj {
    protected SList<IStat> Values { get => Get<SList<IStat>>(0); set => Set<SList<IStat>>(0, value); }
}
[TypeIndex(181)]
public interface IStat : ISerializable, IResource {
}
[TypeIndex(182)]
public interface IStackableStat : IStat {
}
[TypeIndex(183)]
// Should each of these be their own interface?
public interface ITriggeredStat : IStat {
}
[TypeIndex(184)]
public interface IEquatableStat : IStat {
}
[TypeIndex(185)]
public interface IContextualStat : IStat {
}
[TypeIndex(186)]
public interface IInitializableStat : IStat {
}
[TypeIndex(187)]
public interface IViewableStat : IStat {
}

[TypeIndex(188)]
public class SingletonStat : SObj, IEquatableStat {
}
[TypeIndex(189)]
public class StunStat : SIntStat {
}
[SerializableEnum]
[TypeIndex(190)]
// There is already a "Tag" for procgen
public enum Tags : byte {
    Heel,
    Neutral,
    VisionBlocker,
    NaturallyFast,
    NaturallySlow,
    SpacedOut,
    Immobile,
    Inactive,
    InstantDaze,
    SoftDaze,
    DazeRecovery,
    FanProtection,
    DefensePreservation,
    ImmortalChance,
    DiagonalAdjacencies,
    DefenseIsRage,
    TripleMoveResetsToxin,
    ResistConstrictedDamage,
    FlameResistance,
    ExtraFireTurns,
    DamageAdjacent,
    ReflexShove,
    HealWhenMoved,
    ImmortalWithSpirits,
    OverlapObscure,
    DamageOverlap,
    AttackResistant,
    HeavyHitter,
    Minion,
    InspiringLeader,
    Overheating,
    Anomaly,
    Elusive,
    Embarrassed,
    Regenerating,
    TriggeredHealing,
    Flying,
}

[TypeIndex(191)]
public class TacticalTag : Model, ITooltipParent<LogStack> {
    public SEnum<Tags> Tag { get => Get<SEnum<Tags>>(1); set => Set(1, value); }
    [HideInInspector]
    public LogStack TooltipChildren { get => Get<LogStack>(2); set => Set(2, value); }
}
[TypeIndex(192)]
public class TagStat : SList<TacticalTag>, IEquatableStat, IViewableStat, ITriggeredStat {
}
[TypeIndex(193)]
public class TargetStat : SingletonStat, IStat {
    public SInt2 Target { get => Get<SInt2>(0); set => Set(0, value); }
}
[SerializableEnum]
[TypeIndex(194)]
public enum Team : byte {
    Player,
    Monster,
}

[TypeIndex(195)]
public class TeamStat : SObj, IStat {
    public SEnum<Team> Allegiance { get => Get<SEnum<Team>>(0); set => Set(0, value); }
}
[TypeIndex(271)]
public class TemporaryMaxTimeStat : SIntStat, ITriggeredStat {
}
[TypeIndex(196)]
public class TemporaryStrengthStat : SIntStat {
}
[TypeIndex(197)]
public class ToxinStat : SIntStat, ITriggeredStat {
    public SInt MovesThisTurn { get => Get<SInt>(6); set => Set(6, value); }
}
[TypeIndex(270)]
public class TriggeredHealing : TacticalTag, ITriggeredStat {
}
[SerializableEnum]
[TypeIndex(198)]
public enum Unit : byte {
    AIControlled,
    Health,
    Time,
    Defense,
    Rage,
    Toxin,
    Transparent,
    Sprint,
    Attack,
    Accuracy,
    TemporaryAccuracy,
    ChargeDistance,
    Immortal,
    Rock,
    Feud,
    Attention,
    Fuse,
    Daze,
    Fear,
    Stun,
    Fading,
    Confused,
    Stems,
    Constricted,
    Barb,
    Age,
    Prone,
    Bleed,
    Scrying,
    TemporaryMaxTime,
}
[TypeIndex(199)]
public class ViolatesConstraintReason : SObj, IUnusableReason {
    public SString ConstraintName { get => Get<SString>(0); set => Set(0, value); }
    public SBool ViolatesMin { get => Get<SBool>(1); set => Set(1, value); }
    public SBool ViolatesMax { get => Get<SBool>(2); set => Set(2, value); }
}
[TypeIndex(23)]
public class UnchangeableStatReason : SObj, IUnusableReason {
    public SString ConstraintName { get => Get<SString>(0); set => Set(0, value); }
}
[TypeIndex(200)]
public interface IRange : ISerializable {
}
[TypeIndex(201)]
public class Range : SObj, IRange {
    public SBool EnforceMax { get => Get<SBool>(0); set => Set(0, value); }
    public SInt MaxValue { get => Get<SInt>(1); set => Set(1, value); }
    public SBool EnforceMin { get => Get<SBool>(2); set => Set(2, value); }
    public SInt MinValue { get => Get<SInt>(3); set => Set(3, value); }
}
[TypeIndex(212)]
public class InvalidRange : SNull, IRange {
}
[TypeIndex(22)]
public class UnchangeableRange : SNull, IRange {
}
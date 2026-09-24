/**
 * Heart Journey
 * ----------------------------------------
 * Icon Registry
 *
 * These identifiers are stored in Sanity.
 * The Blazor application maps them to the
 * actual icon library (Font Awesome, Fluent,
 * Material, etc.).
 *
 * Never store Font Awesome icon names here.
 */

export const Icons = {
  // General
  Heart: 'heart',
  Cross: 'cross',
  Bible: 'bible',
  Book: 'book',
  BookOpen: 'book-open',
  Compass: 'compass',
  Home: 'home',
  Flag: 'flag',
  Star: 'star',
  Lightbulb: 'lightbulb',
  Shield: 'shield',

  // People
  Person: 'person',
  Couple: 'couple',
  Family: 'family',
  Group: 'group',

  // Growth
  Seedling: 'seedling',
  Tree: 'tree',
  Vine: 'vine',
  Mountain: 'mountain',
  Path: 'path',

  // Relationships
  WeddingRings: 'wedding-rings',
  Conversation: 'conversation',
  Handshake: 'handshake',
  HandsPraying: 'hands-praying',
  Dove: 'dove',

  // Character
  Trust: 'trust',
  Wisdom: 'wisdom',
  Integrity: 'integrity',
  Forgiveness: 'forgiveness',
  Boundaries: 'boundaries',

  // Guidance
  Check: 'check',
  Warning: 'warning',
  Info: 'info',
  Question: 'question'
} as const

export type IconName = (typeof Icons)[keyof typeof Icons]

export const IconList = [
  { title: 'Heart', value: Icons.Heart },
  { title: 'Cross', value: Icons.Cross },
  { title: 'Bible', value: Icons.Bible },
  { title: 'Book', value: Icons.Book },
  { title: 'Book Open', value: Icons.BookOpen },
  { title: 'Compass', value: Icons.Compass },
  { title: 'Home', value: Icons.Home },
  { title: 'Flag', value: Icons.Flag },
  { title: 'Star', value: Icons.Star },
  { title: 'Lightbulb', value: Icons.Lightbulb },
  { title: 'Shield', value: Icons.Shield },

  { title: 'Person', value: Icons.Person },
  { title: 'Couple', value: Icons.Couple },
  { title: 'Family', value: Icons.Family },
  { title: 'Group', value: Icons.Group },

  { title: 'Seedling', value: Icons.Seedling },
  { title: 'Tree', value: Icons.Tree },
  { title: 'Vine', value: Icons.Vine },
  { title: 'Mountain', value: Icons.Mountain },
  { title: 'Path', value: Icons.Path },

  { title: 'Wedding Rings', value: Icons.WeddingRings },
  { title: 'Conversation', value: Icons.Conversation },
  { title: 'Handshake', value: Icons.Handshake },
  { title: 'Hands Praying', value: Icons.HandsPraying },
  { title: 'Dove', value: Icons.Dove },

  { title: 'Trust', value: Icons.Trust },
  { title: 'Wisdom', value: Icons.Wisdom },
  { title: 'Integrity', value: Icons.Integrity },
  { title: 'Forgiveness', value: Icons.Forgiveness },
  { title: 'Boundaries', value: Icons.Boundaries },

  { title: 'Check', value: Icons.Check },
  { title: 'Warning', value: Icons.Warning },
  { title: 'Information', value: Icons.Info },
  { title: 'Question', value: Icons.Question }
]
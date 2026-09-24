import { defineField, defineType } from 'sanity'
import { BookIcon } from '@sanity/icons/Book'

export const bibleVerseType = defineType({
  name: 'bibleVerse',
  title: 'Bible Verse',
  type: 'document',
  icon: BookIcon,

  fieldsets: [
    {
      name: 'reference',
      title: 'Reference'
    },
    {
      name: 'content',
      title: 'Content'
    },
    {
      name: 'context',
      title: 'Context'
    },
    {
      name: 'publishing',
      title: 'Publishing'
    }
  ],

  fields: [

    // -------------------------
    // Reference
    // -------------------------

    defineField({
      name: 'book',
      title: 'Book',
      type: 'string',
      fieldset: 'reference',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'chapter',
      title: 'Chapter',
      type: 'number',
      fieldset: 'reference',
      validation: Rule => Rule.required().min(1)
    }),

    defineField({
      name: 'verseStart',
      title: 'Verse Start',
      type: 'number',
      fieldset: 'reference',
      validation: Rule => Rule.required().min(1)
    }),

    defineField({
      name: 'verseEnd',
      title: 'Verse End (optional)',
      type: 'number',
      fieldset: 'reference'
    }),

    defineField({
      name: 'translation',
      title: 'Translation',
      type: 'string',
      fieldset: 'reference',
      options: {
        list: [
          { title: 'NIV', value: 'NIV' },
          { title: 'ESV', value: 'ESV' },
          { title: 'KJV', value: 'KJV' },
          { title: 'NKJV', value: 'NKJV' }
        ]
      },
      initialValue: 'NIV',
      validation: Rule => Rule.required()
    }),

    // -------------------------
    // Content
    // -------------------------

    defineField({
      name: 'text',
      title: 'Verse Text',
      type: 'text',
      rows: 4,
      fieldset: 'content',
      validation: Rule => Rule.required()
    }),

    // -------------------------
    // Context
    // -------------------------

    defineField({
      name: 'theme',
      title: 'Theme',
      type: 'string',
      fieldset: 'context',
      description: 'e.g. communication, patience, love, wisdom'
    }),

    defineField({
      name: 'tags',
      title: 'Tags',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'context'
    }),

    // -------------------------
    // Publishing
    // -------------------------

    defineField({
      name: 'published',
      title: 'Published',
      type: 'boolean',
      initialValue: false,
      fieldset: 'publishing'
    })

  ],

  preview: {
    select: {
      book: 'book',
      chapter: 'chapter',
      verseStart: 'verseStart',
      verseEnd: 'verseEnd'
    },
    prepare({ book, chapter, verseStart, verseEnd }) {
      const reference = verseEnd
        ? `${book} ${chapter}:${verseStart}-${verseEnd}`
        : `${book} ${chapter}:${verseStart}`

      return {
        title: reference,
        subtitle: 'Bible Verse'
      }
    }
  }
})
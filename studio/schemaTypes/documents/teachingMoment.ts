import { defineField, defineType } from 'sanity'
import { BookIcon } from '@sanity/icons/Book'

export const teachingMomentType = defineType({
  name: 'teachingMoment',
  title: 'Teaching Moment',
  type: 'document',
  icon: BookIcon,

  fieldsets: [
    {
      name: 'identity',
      title: 'Identity'
    },
    {
      name: 'context',
      title: 'Context'
    },
    {
      name: 'teaching',
      title: 'Teaching'
    },
    {
      name: 'guidance',
      title: 'Guidance'
    },
    {
      name: 'publishing',
      title: 'Publishing'
    }
  ],

  fields: [

    // -------------------------
    // Identity
    // -------------------------

    defineField({
      name: 'title',
      title: 'Title',
      type: 'string',
      fieldset: 'identity',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'slug',
      title: 'Slug',
      type: 'slug',
      fieldset: 'identity',
      options: {
        source: 'title',
        maxLength: 96
      },
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'type',
      title: 'Teaching Type',
      type: 'string',
      fieldset: 'identity',
      options: {
        list: [
          { title: 'Biblical Teaching', value: 'biblical' },
          { title: 'Relationship Wisdom', value: 'relationship' },
          { title: 'Warning / Discernment', value: 'warning' },
          { title: 'Encouragement', value: 'encouragement' }
        ]
      },
      initialValue: 'relationship',
      validation: Rule => Rule.required()
    }),

    // -------------------------
    // Context
    // -------------------------

    defineField({
      name: 'milestone',
      title: 'Milestone',
      type: 'reference',
      to: [{ type: 'milestone' }],
      fieldset: 'context',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'dimension',
      title: 'Dimension',
      type: 'reference',
      to: [{ type: 'dimension' }],
      fieldset: 'context'
    }),

    defineField({
      name: 'topic',
      title: 'Topic',
      type: 'string',
      fieldset: 'context'
    }),

    // -------------------------
    // Teaching Content
    // -------------------------

    defineField({
      name: 'teaching',
      title: 'Teaching',
      type: 'array',
      of: [{ type: 'block' }],
      fieldset: 'teaching',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'keyTakeaway',
      title: 'Key Takeaway',
      type: 'text',
      rows: 3,
      fieldset: 'teaching'
    }),

    defineField({
      name: 'scriptureReference',
      title: 'Scripture Reference',
      type: 'string',
      fieldset: 'teaching'
    }),

    defineField({
      name: 'scriptureText',
      title: 'Scripture Text',
      type: 'text',
      rows: 4,
      fieldset: 'teaching'
    }),

    // -------------------------
    // Guidance
    // -------------------------

    defineField({
      name: 'practicalApplication',
      title: 'Practical Application',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'guidance'
    }),

    defineField({
      name: 'tone',
      title: 'Tone',
      type: 'string',
      fieldset: 'guidance',
      options: {
        list: [
          { title: 'Gentle', value: 'gentle' },
          { title: 'Direct', value: 'direct' },
          { title: 'Warning', value: 'warning' },
          { title: 'Encouraging', value: 'encouraging' }
        ]
      },
      initialValue: 'gentle'
    }),

    // -------------------------
    // Publishing
    // -------------------------

    defineField({
      name: 'sortOrder',
      title: 'Sort Order',
      type: 'number',
      validation: Rule => Rule.required().min(1),
      fieldset: 'publishing'
    }),

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
      title: 'title',
      subtitle: 'milestone.title'
    }
  }
})
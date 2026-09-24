import { defineField, defineType } from 'sanity'
import { BulbOutlineIcon } from '@sanity/icons/BulbOutline'

export const insightType = defineType({
  name: 'insight',
  title: 'Insight',
  type: 'document',
  icon: BulbOutlineIcon,

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
      name: 'content',
      title: 'Insight'
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
      title: 'Insight Type',
      type: 'string',
      fieldset: 'identity',
      options: {
        list: [
          { title: 'General Insight', value: 'general' },
          { title: 'Discernment Insight', value: 'discernment' },
          { title: 'Encouragement Insight', value: 'encouragement' }
        ]
      },
      initialValue: 'general',
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
      name: 'reflectionPrompt',
      title: 'Reflection Prompt',
      type: 'reference',
      to: [{ type: 'reflectionPrompt' }],
      fieldset: 'context'
    }),

    // -------------------------
    // Insight Content
    // -------------------------

    defineField({
      name: 'insight',
      title: 'Insight',
      type: 'text',
      rows: 6,
      fieldset: 'content',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'scriptureReference',
      title: 'Scripture Reference',
      type: 'string',
      fieldset: 'content'
    }),

    defineField({
      name: 'scriptureText',
      title: 'Scripture Text',
      type: 'text',
      rows: 4,
      fieldset: 'content'
    }),

    // -------------------------
    // Guidance
    // -------------------------

    defineField({
      name: 'practicalTakeaways',
      title: 'Practical Takeaways',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'guidance'
    }),

    defineField({
      name: 'spiritualReflection',
      title: 'Spiritual Reflection',
      type: 'text',
      rows: 4,
      fieldset: 'guidance'
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
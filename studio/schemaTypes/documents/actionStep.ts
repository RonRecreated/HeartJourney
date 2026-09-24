import { defineField, defineType } from 'sanity'
import { CheckmarkCircleIcon } from '@sanity/icons/CheckmarkCircle'

export const actionStepType = defineType({
  name: 'actionStep',
  title: 'Action Step',
  type: 'document',
  icon: CheckmarkCircleIcon,

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
      name: 'action',
      title: 'Action Step'
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
      title: 'Action Type',
      type: 'string',
      fieldset: 'identity',
      options: {
        list: [
          { title: 'Communication', value: 'communication' },
          { title: 'Emotional Growth', value: 'emotional' },
          { title: 'Spiritual Practice', value: 'spiritual' },
          { title: 'Behavioral', value: 'behavioral' }
        ]
      },
      initialValue: 'behavioral',
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
      name: 'insight',
      title: 'Insight',
      type: 'reference',
      to: [{ type: 'insight' }],
      fieldset: 'context'
    }),

    // -------------------------
    // Action Content
    // -------------------------

    defineField({
      name: 'instruction',
      title: 'Instruction',
      type: 'text',
      rows: 3,
      fieldset: 'action',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'whyItMatters',
      title: 'Why It Matters',
      type: 'text',
      rows: 3,
      fieldset: 'action'
    }),

    defineField({
      name: 'example',
      title: 'Example',
      type: 'text',
      rows: 3,
      fieldset: 'action'
    }),

    // -------------------------
    // Guidance
    // -------------------------

    defineField({
      name: 'difficulty',
      title: 'Difficulty',
      type: 'string',
      fieldset: 'guidance',
      options: {
        list: [
          { title: 'Light', value: 'light' },
          { title: 'Moderate', value: 'moderate' },
          { title: 'Deep', value: 'deep' }
        ]
      },
      initialValue: 'light'
    }),

    defineField({
      name: 'reflectionAfterAction',
      title: 'Reflection After Action',
      type: 'text',
      rows: 3,
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